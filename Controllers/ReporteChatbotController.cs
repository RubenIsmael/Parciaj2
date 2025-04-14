using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Serialization;

namespace San_Agustin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteChatbotController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _connectionString;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _apiEndpoint;

        public ReporteChatbotController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _apiKey = _configuration["Openai:ApiKey"];
            _model = _configuration["Openai:Model"];
            _apiEndpoint = _configuration["Openai:ApiEndpoint"];
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            try
            {
                // Validar la solicitud
                if (string.IsNullOrEmpty(request.Message))
                {
                    return BadRequest("El mensaje no puede estar vacío");
                }

                // Analizar si se solicita información de la base de datos
                bool requiresData = NeedsDataQuery(request.Message);
                string contextData = string.Empty;

                // Si se requiere información de la base de datos, obtenerla
                if (requiresData)
                {
                    contextData = await GetDatabaseInfo(request.Message);
                }

                // Verificar si se solicita una gráfica
                bool requiresChart = RequestsChart(request.Message);

                // Construir el prompt para GPT
                var messages = new List<object>
                {
                    new { role = "system", content = GetSystemPrompt(requiresData, requiresChart) }
                };

                // Agregar contexto de base de datos si existe
                if (!string.IsNullOrEmpty(contextData))
                {
                    messages.Add(new { role = "system", content = $"Datos de la base de datos: {contextData}" });
                }

                // Agregar mensaje del usuario
                messages.Add(new { role = "user", content = request.Message });

                // Preparar la solicitud para OpenAI
                var openAiRequest = new
                {
                    model = _model,
                    messages,
                    temperature = 0.7,
                    max_tokens = 1000
                };

                // Enviar solicitud a OpenAI
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var content = new StringContent(
                    JsonSerializer.Serialize(openAiRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(_apiEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Error en la API de OpenAI: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);

                var botResponse = openAiResponse?.Choices?[0]?.Message?.Content;

                if (string.IsNullOrEmpty(botResponse))
                {
                    return StatusCode(500, "No se recibió una respuesta válida de OpenAI");
                }

                // Procesar la respuesta para identificar si contiene una solicitud de gráfica
                var processedResponse = ProcessBotResponse(botResponse, requiresChart);
                return Ok(processedResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private bool NeedsDataQuery(string message)
        {
            // Palabras clave que indican que se necesita consultar la base de datos
            string[] dataKeywords = new[] {
                "cuántas", "cuántos", "disponible", "disponibles", "reserva", "reservas",
                "bóveda", "bóvedas", "bobeda", "bobedas", "contrato", "contratos",
                "pago", "pagos", "cliente", "clientes", "sector", "sectores",
                "precio", "precios", "estado", "estados", "total", "ocupación",
                "ocupadas", "libres", "datos", "información", "estadísticas"
            };

            message = message.ToLower();

            foreach (var keyword in dataKeywords)
            {
                if (message.Contains(keyword.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private bool RequestsChart(string message)
        {
            // Palabras clave que indican solicitud de gráfica
            string[] chartKeywords = new[] {
                "gráfica", "grafica", "gráfico", "grafico", "chart", "diagrama",
                "visualización", "visualizacion", "mostrar gráficamente", "estadística",
                "comparación", "comparacion", "tendencia", "porcentaje", "distribución"
            };

            message = message.ToLower();

            foreach (var keyword in chartKeywords)
            {
                if (message.Contains(keyword.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetSystemPrompt(bool requiresData, bool requiresChart)
        {
            string basePrompt = @"Eres un asistente virtual para el Cementerio San Agustín. Tu objetivo es proporcionar información precisa y útil sobre bóvedas, reservas, contratos y estadísticas.
            
Responde de manera amable, clara y profesional. Utiliza un tono respetuoso que sea apropiado para el contexto de un cementerio.

Si te preguntan por información específica que no tienes, indica que necesitarías consultar la base de datos para proporcionar esa información exacta.";

            if (requiresData)
            {
                basePrompt += "\n\nTienes acceso a datos de la base de datos que se te proporcionarán como contexto. Utiliza esta información para responder con precisión.";
            }

            if (requiresChart)
            {
                basePrompt += @"

El usuario ha solicitado una gráfica o visualización de datos. Si necesitas generar una gráfica, debes incluir en tu respuesta un bloque JSON con la siguiente estructura:

```json
{
  ""generateChart"": true,
  ""chartType"": ""bar|line|pie|doughnut"",
  ""chartData"": {
    ""labels"": [""Etiqueta1"", ""Etiqueta2"", ...],
    ""datasets"": [
      {
        ""label"": ""Nombre del conjunto de datos"",
        ""data"": [valor1, valor2, ...],
        ""backgroundColor"": [""#color1"", ""#color2"", ...],
        ""borderColor"": ""#colorBorde"",
        ""borderWidth"": 1
      }
    ]
  },
  ""chartOptions"": {
    // Opciones adicionales de configuración para la gráfica
  }
}
```

No olvides explicar también en texto lo que muestra la gráfica antes y/o después del bloque JSON.";
            }

            return basePrompt;
        }

        private async Task<string> GetDatabaseInfo(string userQuery)
        {
            try
            {
                // Analizamos la consulta del usuario para determinar qué datos obtener
                if (userQuery.ToLower().Contains("bóveda") || userQuery.ToLower().Contains("bobeda"))
                {
                    // Consulta sobre bóvedas
                    return await GetBovedasInfo();
                }
                else if (userQuery.ToLower().Contains("reserva"))
                {
                    // Consulta sobre reservas
                    return await GetReservasInfo();
                }
                else if (userQuery.ToLower().Contains("contrato"))
                {
                    // Consulta sobre contratos
                    return await GetContratosInfo();
                }
                else if (userQuery.ToLower().Contains("precio") || userQuery.ToLower().Contains("tarifas"))
                {
                    // Consulta sobre precios
                    return await GetPreciosInfo();
                }
                else if (userQuery.ToLower().Contains("estadística") || userQuery.ToLower().Contains("estadistica"))
                {
                    // Consulta sobre estadísticas generales
                    return await GetEstadisticasGenerales();
                }

                // Si no hay coincidencia específica, obtener un resumen general
                return await GetResumenGeneral();
            }
            catch (Exception ex)
            {
                return $"Error al consultar la base de datos: {ex.Message}";
            }
        }

        private async Task<string> GetBovedasInfo()
        {
            var result = new StringBuilder();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Obtener resumen de bóvedas por estado
                using (var command = new SqlCommand(@"
                    SELECT Estado, COUNT(*) as Cantidad 
                    FROM Bobedas 
                    GROUP BY Estado 
                    ORDER BY Cantidad DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("Resumen de bóvedas por estado:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["Estado"]}: {reader["Cantidad"]} bóvedas");
                        }
                    }
                }

                // Obtener resumen de bóvedas por sector
                using (var command = new SqlCommand(@"
                    SELECT Division, Estado
                    FROM Bobedas 
                    ORDER BY Division", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("\nDetalle de bóvedas:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["Division"]}: {reader["Estado"]}");
                        }
                    }
                }
            }

            return result.ToString();
        }

        private async Task<string> GetReservasInfo()
        {
            var result = new StringBuilder();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Obtener resumen de reservas por estado
                using (var command = new SqlCommand(@"
                    SELECT EstadoPago, COUNT(*) as Cantidad 
                    FROM Reservas 
                    GROUP BY EstadoPago 
                    ORDER BY Cantidad DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("Resumen de reservas por estado de pago:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["EstadoPago"]}: {reader["Cantidad"]} reservas");
                        }
                    }
                }

                // Obtener resumen de reservas por fecha
                using (var command = new SqlCommand(@"
                    SELECT FORMAT(FechaReserva, 'yyyy-MM') as Mes, COUNT(*) as Cantidad 
                    FROM Reservas 
                    GROUP BY FORMAT(FechaReserva, 'yyyy-MM') 
                    ORDER BY Mes", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("\nReservas por mes:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["Mes"]}: {reader["Cantidad"]} reservas");
                        }
                    }
                }

                // Obtener últimas reservas realizadas
                using (var command = new SqlCommand(@"
                    SELECT TOP 5 r.Id, r.Nombre + ' ' + r.Apellido as Cliente, p.Sector, r.FechaReserva, r.EstadoPago
                    FROM Reservas r
                    JOIN Precios p ON r.IdPrecio = p.Id
                    ORDER BY r.FechaReserva DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("\nÚltimas reservas realizadas:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- ID: {reader["Id"]}, Cliente: {reader["Cliente"]}, Sector: {reader["Sector"]}, Fecha: {reader["FechaReserva"]}, Estado: {reader["EstadoPago"]}");
                        }
                    }
                }
            }

            return result.ToString();
        }

        private async Task<string> GetContratosInfo()
        {
            var result = new StringBuilder();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Obtener información de contratos
                using (var command = new SqlCommand(@"
                    SELECT c.Id, r.Nombre + ' ' + r.Apellido as Cliente, c.FechaInicio, c.FechaFin, c.Terminos
                    FROM Contratos c
                    JOIN Reservas r ON c.IdReserva = r.Id
                    ORDER BY c.FechaInicio DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("Información de contratos:");
                        bool hasData = false;
                        while (await reader.ReadAsync())
                        {
                            hasData = true;
                            result.AppendLine($"- ID: {reader["Id"]}, Cliente: {reader["Cliente"]}");
                            result.AppendLine($"  Desde: {reader["FechaInicio"]} hasta: {reader["FechaFin"]}");
                            result.AppendLine($"  Términos: {reader["Terminos"]}");
                            result.AppendLine();
                        }

                        if (!hasData)
                        {
                            result.AppendLine("No se encontraron contratos en la base de datos.");
                        }
                    }
                }

                // Contratos activos vs vencidos
                using (var command = new SqlCommand(@"
                    SELECT 
                        CASE WHEN FechaFin >= GETDATE() THEN 'Activo' ELSE 'Vencido' END as Estado,
                        COUNT(*) as Cantidad 
                    FROM Contratos 
                    GROUP BY CASE WHEN FechaFin >= GETDATE() THEN 'Activo' ELSE 'Vencido' END", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        bool hasData = false;
                        result.AppendLine("\nEstado de contratos:");
                        while (await reader.ReadAsync())
                        {
                            hasData = true;
                            result.AppendLine($"- {reader["Estado"]}: {reader["Cantidad"]} contratos");
                        }

                        if (!hasData)
                        {
                            result.AppendLine("No hay información sobre el estado de contratos.");
                        }
                    }
                }
            }

            return result.ToString();
        }

        private async Task<string> GetPreciosInfo()
        {
            var result = new StringBuilder();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Obtener listado de precios
                using (var command = new SqlCommand(@"
                    SELECT p.Sector, p.PrecioValor, t.Descripcion
                    FROM Precios p
                    LEFT JOIN TipoBobeda t ON p.Sector = t.Nombre
                    ORDER BY p.PrecioValor DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("Listado de precios por sector:");
                        while (await reader.ReadAsync())
                        {
                            string descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : "Sin descripción";
                            result.AppendLine($"- {reader["Sector"]}: ${reader["PrecioValor"]} - {descripcion}");
                        }
                    }
                }

                // Obtener estadísticas de precios
                using (var command = new SqlCommand(@"
                    SELECT 
                        MIN(PrecioValor) as Minimo,
                        MAX(PrecioValor) as Maximo,
                        AVG(PrecioValor) as Promedio
                    FROM Precios", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.AppendLine("\nEstadísticas de precios:");
                            result.AppendLine($"- Precio mínimo: ${reader["Minimo"]}");
                            result.AppendLine($"- Precio máximo: ${reader["Maximo"]}");
                            result.AppendLine($"- Precio promedio: ${reader["Promedio"]}");
                        }
                    }
                }
            }

            return result.ToString();
        }

        private async Task<string> GetEstadisticasGenerales()
        {
            var result = new StringBuilder();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Estadísticas de bóvedas
                using (var command = new SqlCommand(@"
                    SELECT Estado, COUNT(*) as Cantidad 
                    FROM Bobedas 
                    GROUP BY Estado 
                    ORDER BY Cantidad DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("Estadísticas de bóvedas por estado:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["Estado"]}: {reader["Cantidad"]} bóvedas");
                        }
                    }
                }

                // Estadísticas de reservas
                using (var command = new SqlCommand(@"
                    SELECT EstadoPago, COUNT(*) as Cantidad 
                    FROM Reservas 
                    GROUP BY EstadoPago", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("\nEstadísticas de reservas por estado:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["EstadoPago"]}: {reader["Cantidad"]} reservas");
                        }
                    }
                }

                // Clientes registrados
                using (var command = new SqlCommand("SELECT COUNT(*) as Total FROM Clientes", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.AppendLine($"\nTotal de clientes registrados: {reader["Total"]}");
                        }
                    }
                }

                // Rango de precios
                using (var command = new SqlCommand(@"
                    SELECT 
                        MIN(PrecioValor) as Minimo,
                        MAX(PrecioValor) as Maximo,
                        AVG(PrecioValor) as Promedio
                    FROM Precios", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.AppendLine("\nRango de precios:");
                            result.AppendLine($"- Mínimo: ${reader["Minimo"]}");
                            result.AppendLine($"- Máximo: ${reader["Maximo"]}");
                            result.AppendLine($"- Promedio: ${reader["Promedio"]}");
                        }
                    }
                }
            }

            return result.ToString();
        }

        private async Task<string> GetResumenGeneral()
        {
            var result = new StringBuilder();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Total de bóvedas
                using (var command = new SqlCommand("SELECT COUNT(*) as Total FROM Bobedas", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.AppendLine($"Total de bóvedas: {reader["Total"]}");
                        }
                    }
                }

                // Bóvedas por estado
                using (var command = new SqlCommand(@"
                    SELECT Estado, COUNT(*) as Cantidad 
                    FROM Bobedas 
                    GROUP BY Estado", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("\nBóvedas por estado:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["Estado"]}: {reader["Cantidad"]}");
                        }
                    }
                }

                // Total de reservas
                using (var command = new SqlCommand("SELECT COUNT(*) as Total FROM Reservas", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.AppendLine($"\nTotal de reservas: {reader["Total"]}");
                        }
                    }
                }

                // Reservas por estado de pago
                using (var command = new SqlCommand(@"
                    SELECT EstadoPago, COUNT(*) as Cantidad 
                    FROM Reservas 
                    GROUP BY EstadoPago", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        result.AppendLine("\nReservas por estado de pago:");
                        while (await reader.ReadAsync())
                        {
                            result.AppendLine($"- {reader["EstadoPago"]}: {reader["Cantidad"]}");
                        }
                    }
                }

                // Total de clientes
                using (var command = new SqlCommand("SELECT COUNT(*) as Total FROM Clientes", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            result.AppendLine($"\nTotal de clientes registrados: {reader["Total"]}");
                        }
                    }
                }
            }

            return result.ToString();
        }

        private object ProcessBotResponse(string botResponse, bool requiresChart)
        {
            if (!requiresChart)
            {
                return new { message = botResponse, hasChart = false };
            }

            // Verificar si la respuesta contiene un bloque JSON para la gráfica
            int startIndex = botResponse.IndexOf("```json");
            int endIndex = botResponse.LastIndexOf("```");

            if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
            {
                // Extraer el JSON
                startIndex += 7; // Longitud de "```json"
                string jsonContent = botResponse.Substring(startIndex, endIndex - startIndex).Trim();

                try
                {
                    // Parsear el JSON
                    var chartData = JsonDocument.Parse(jsonContent);

                    // Extraer el texto explicativo (antes y después del JSON)
                    string textBefore = botResponse.Substring(0, botResponse.IndexOf("```json")).Trim();
                    string textAfter = botResponse.Substring(endIndex + 3).Trim(); // +3 para omitir los ```

                    return new
                    {
                        message = $"{textBefore}\n\n{textAfter}",
                        hasChart = true,
                        chartData = chartData
                    };
                }
                catch (Exception)
                {
                    // Si hay un error al parsear el JSON, devolver la respuesta original
                    return new { message = botResponse, hasChart = false };
                }
            }

            return new { message = botResponse, hasChart = false };
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }

    public class OpenAiResponse
    {
        public Choice[] Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
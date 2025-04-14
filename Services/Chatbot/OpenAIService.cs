using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using Microsoft.Win32;
using Newtonsoft.Json;
using San_Agustin_Final.Models;

namespace San_Agustin_Final.Services.ChatBot
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ChatbotSettings _settings;
        private readonly DatabaseQueryService _databaseService;

        public OpenAIService(IHttpClientFactory httpClientFactory, IOptions<ChatbotSettings> settings, DatabaseQueryService databaseService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _settings = settings.Value;
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
            _databaseService = databaseService;
        }

        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            try
            {
                Console.WriteLine($"Procesando mensaje: {userMessage}");
                Console.WriteLine($"API Key: {_settings.ApiKey.Substring(0, 10)}...");
                Console.WriteLine($"Modelo: {_settings.Model}");
                Console.WriteLine($"Endpoint: {_settings.ApiEndpoint}");

                // Procesar el mensaje para ver si necesita consultar la base de datos
                string databaseData = await ProcessDatabaseRequest(userMessage);

                string systemMessage = "Eres un asistente virtual para el cementerio San Agustín. " +
                                      "Ayudas a los clientes con información sobre bóvedas, reservas, pagos y otros servicios. " +
                                      "Sé amable, respetuoso y empático. " +
                                      "Si te preguntan por disponibilidad de bóvedas, precios o información de pagos, usa la información de la base de datos."
                                      + "Usa oraciones cortas y claras."+"Separa los párrafos con saltos de línea."
                                      +"Organiza la información en secciones claras."+"Evita respuestas demasiado largas."+"Prioriza la información más relevante."
+"Si no puedes responder con precisión, sugiere que el usuario contacte directamente con el cementerio."+"Si el usuario escribe con errores ortográficos o preguntas mal formuladas, intenta interpretar su intención y responde de la mejor manera posible."
+"Si la consulta requiere datos de la base de datos, ejecuta la consulta y devuelve solo los resultados, sin mostrar el código SQL."+"Si la consulta es ambigua, solicita más detalles antes de dar una respuesta."+
"Si la pregunta tiene múltiples interpretaciones, ofrece opciones para que el usuario elija."+"Si el usuario solicita información que no puede obtenerse, informa con claridad y sugiere alternativas."+"Si una consulta puede generar un error o afectar el rendimiento, advierte antes de ejecutarla y solicita confirmación."+
"Prioriza respuestas prácticas y accionables en lugar de teoría extensa."+"Si la consulta involucra información confidencial, evita mostrar datos sensibles y solicita autenticación si es necesario."+"Mantén coherencia en los términos utilizados para evitar confusión en respuestas sucesivas."+
"Si el usuario consulta el estado de pago de una bóveda, devuelve solo la información relevante(estado: pagado, pendiente, vencido), sin detalles innecesarios."+"Si un pago está vencido, informa al usuario y sugiere los pasos para regularizarlo."+
"Si se consulta la disponibilidad de bóvedas, muestra solo las bóvedas disponibles y sus datos esenciales(ubicación, tipo, costo)."+"Si el usuario solicita datos personales de fallecidos o titulares de bóvedas, valida permisos antes de proporcionar información."+
"Si un usuario intenta registrar un pago, verifica que los datos sean correctos antes de procesar la transacción."+"Si una consulta requiere autenticación(como actualizar datos o registrar pagos), solicita credenciales antes de proceder."+
"Si el usuario intenta eliminar o modificar registros importantes, solicita doble confirmación."+"Evita mostrar datos sensibles(como montos exactos, información de contacto o documentos personales) sin autorización previa."+"Si el sistema detecta errores en la base de datos(duplicados, pagos inconsistentes, etc.), informa al usuario y sugiere contactar al administrador."+
"Si se solicita ayuda o soporte, proporciona una respuesta clara y un contacto del administrador si es necesario." ;

                if (!string.IsNullOrEmpty(databaseData))
                {
                    systemMessage += $"\n\nInformación de la base de datos: {databaseData}";
                }

                var messages = new List<object>
        {
            new { role = "system", content = systemMessage },
            new { role = "user", content = userMessage }
        };

                var requestData = new
                {
                    model = _settings.Model,
                    messages = messages,
                    max_tokens = 500,
                    temperature = 0.7
                };

                var jsonContent = JsonConvert.SerializeObject(requestData);
                Console.WriteLine($"Enviando solicitud a OpenAI: {jsonContent}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_settings.ApiEndpoint, content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Respuesta recibida: {responseBody}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error en la respuesta: {response.StatusCode}");
                    Console.WriteLine($"Detalles: {responseBody}");
                    return $"Error al comunicarse con OpenAI: {response.StatusCode} - {responseBody}";
                }

                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseBody);

                return responseObject.choices[0].message.content.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al comunicarse con OpenAI: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return $"Error al procesar la solicitud: {ex.Message}";
            }
        }

        private async Task<string> ProcessDatabaseRequest(string userMessage)
        {
            string lowercaseMessage = userMessage.ToLower();

            // Detectar preguntas relacionadas con la base de datos
            if (lowercaseMessage.Contains("bóvedas disponibles") || lowercaseMessage.Contains("bobedas disponibles"))
            {
                return await _databaseService.GetAvailableBovedasAsync();
            }
            else if (lowercaseMessage.Contains("precio") || lowercaseMessage.Contains("costo"))
            {
                return await _databaseService.GetPricesAsync();
            }
            else if (lowercaseMessage.Contains("pago") || lowercaseMessage.Contains("pagos pendientes"))
            {
                // Aquí podríamos pedir un identificador al usuario
                if (lowercaseMessage.Contains("pendiente"))
                {
                    return await _databaseService.GetPendingPaymentsAsync();
                }
                return await _databaseService.GetPaymentHistoryAsync();
            }
            else if (lowercaseMessage.Contains("reserva") || lowercaseMessage.Contains("reservar"))
            {
                return await _databaseService.GetReservationInfoAsync();
            }
            else if (lowercaseMessage.Contains("cliente") || lowercaseMessage.Contains("clientes"))
            {
                return await _databaseService.GetClientsInfoAsync();
            }

            return string.Empty; // No se requiere información de la base de datos
        }
    }
}
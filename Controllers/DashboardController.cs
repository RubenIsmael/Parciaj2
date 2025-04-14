using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using San_Agustin_Final.Models; 
using San_Agustin_Final.Data;


namespace San_Agustin_Final.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(); // Asegúrate de tener la vista en Views/Dashboard/Index.cshtml
        }
    
            [HttpGet("GetData")]
            public async Task<IActionResult> GetData(string question, DateTime startDate, DateTime endDate)
            {
                try
                {
                    // Asegurarse de que las fechas son válidas
                    if (startDate > endDate)
                    {
                        var temp = startDate;
                        startDate = endDate;
                        endDate = temp;
                    }

                    // Ajustar fecha de fin para incluir todo el día
                    endDate = endDate.AddDays(1).AddTicks(-1);

                    // Responder según la pregunta
                    switch (question)
                    {
                        case "reservas-periodo":
                            return Ok(await GetReservasPorPeriodo(startDate, endDate));
                        case "estado-reservas":
                            return Ok(await GetEstadoReservas(startDate, endDate));
                        case "clientes-reservas":
                            return Ok(await GetClientesReservas(startDate, endDate));
                        case "promedio-cliente":
                            return Ok(await GetPromedioCliente(startDate, endDate));
                        case "ingresos-periodo":
                            return Ok(await GetIngresosPeriodo(startDate, endDate));
                        case "pagos-promedio":
                            return Ok(await GetPagosPromedio(startDate, endDate));
                        case "bobedas-estado":
                            return Ok(await GetBobedasEstado());
                        case "precio-promedio":
                            return Ok(await GetPrecioPromedio());
                        case "bobeda-demanda":
                            return Ok(await GetBobedaDemanda(startDate, endDate));
                        case "mensajes-estado":
                            return Ok(await GetMensajesEstado(startDate, endDate));
                        case "tendencias-tiempo":
                            return Ok(await GetTendenciasTiempo(startDate, endDate));
                        case "meses-actividad":
                            return Ok(await GetMesesActividad(startDate, endDate));
                        default:
                            return BadRequest("Pregunta no reconocida");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error interno: {ex.Message}");
                }
            }

            private async Task<object> GetReservasPorPeriodo(DateTime startDate, DateTime endDate)
            {
                // Obtener todas las reservas en el período especificado
                var reservas = await _context.Reservas
                    .Where(r => r.FechaReserva >= startDate && r.FechaReserva <= endDate)
                    .ToListAsync();

                // Agrupar por mes
                var reservasPorMes = reservas
                    .GroupBy(r => new { r.FechaReserva.Year, r.FechaReserva.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Count = g.Count()
                    })
                    .OrderBy(g => g.Year)
                    .ThenBy(g => g.Month)
                    .ToList();

                // Obtener el total de reservas
                int totalReservas = reservas.Count;

                // Calcular el promedio mensual
                double promedioMensual = totalReservas;
                if (reservasPorMes.Count > 0)
                {
                    promedioMensual = Math.Round((double)totalReservas / reservasPorMes.Count, 1);
                }

                // Preparar datos para el gráfico principal
                var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                var datos = new int[12];

                foreach (var item in reservasPorMes)
                {
                    datos[item.Month - 1] = item.Count;
                }

                // Calcular crecimiento (ejemplo simple)
                string crecimiento = "0%";
                if (reservasPorMes.Count >= 2)
                {
                    var primerMes = reservasPorMes.First().Count;
                    var ultimoMes = reservasPorMes.Last().Count;
                    if (primerMes > 0)
                    {
                        var porcentaje = Math.Round(((double)ultimoMes / primerMes - 1) * 100);
                        crecimiento = $"{porcentaje}%";
                    }
                }

                // Preparar datos para gráfico secundario (por trimestre)
                var datosPorTrimestre = new[]
                {
                datos[0] + datos[1] + datos[2],     // Q1
                datos[3] + datos[4] + datos[5],     // Q2
                datos[6] + datos[7] + datos[8],     // Q3
                datos[9] + datos[10] + datos[11]    // Q4
            };

                // Preparar datos para tabla
                var tablaDatos = new List<object>();
                for (int i = 0; i < 12; i++)
                {
                    if (datos[i] > 0)
                    {
                        tablaDatos.Add(new
                        {
                            categoria = meses[i],
                            valor = $"{datos[i]} reservas"
                        });
                    }
                }

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Total Reservas", value = totalReservas.ToString(), description = "en el período seleccionado" },
                    new { title = "Promedio Mensual", value = promedioMensual.ToString(), description = "reservas por mes" },
                    new { title = "Crecimiento", value = crecimiento, description = "vs. período anterior" }
                },
                    chartData = new
                    {
                        categories = meses,
                        series = new[]
                        {
                        new { name = "Reservas", data = datos }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = new[] { "Q1", "Q2", "Q3", "Q4" },
                        series = datosPorTrimestre
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetEstadoReservas(DateTime startDate, DateTime endDate)
            {
                // Obtener todas las reservas en el período especificado
                var reservas = await _context.Reservas
                    .Where(r => r.FechaReserva >= startDate && r.FechaReserva <= endDate)
                    .ToListAsync();

                // Agrupar por estado de pago
                var estadoReservas = reservas
                    .GroupBy(r => r.EstadoPago)
                    .Select(g => new
                    {
                        Estado = g.Key,
                        Count = g.Count(),
                        Porcentaje = reservas.Count > 0 ? Math.Round((double)g.Count() / reservas.Count * 100, 1) : 0
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList();

                // Preparar datos para los KPIs
                int totalReservas = reservas.Count;
                int reservasPagadas = estadoReservas.FirstOrDefault(e => e.Estado.ToLower() == "pagado")?.Count ?? 0;
                int reservasPendientes = estadoReservas.FirstOrDefault(e => e.Estado.ToLower() == "pendiente")?.Count ?? 0;

                double porcentajePagadas = totalReservas > 0 ? Math.Round((double)reservasPagadas / totalReservas * 100, 1) : 0;
                string porcentajePagadasStr = $"{porcentajePagadas}%";

                // Preparar datos para el gráfico principal
                var estados = estadoReservas.Select(e => e.Estado).ToArray();
                var cantidades = estadoReservas.Select(e => e.Count).ToArray();

                // Preparar datos para gráfico secundario (distribución)
                var seriesDistribucion = estadoReservas.Select(e => e.Count).ToArray();
                var labelsDistribucion = estadoReservas.Select(e => e.Estado).ToArray();

                // Preparar datos para tabla
                var tablaDatos = estadoReservas.Select(e => new
                {
                    categoria = e.Estado,
                    valor = $"{e.Count} reservas ({e.Porcentaje}%)"
                }).ToList<object>();

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Total Reservas", value = totalReservas.ToString(), description = "en el período seleccionado" },
                    new { title = "Reservas Pagadas", value = reservasPagadas.ToString(), description = $"{porcentajePagadasStr} del total" },
                    new { title = "Reservas Pendientes", value = reservasPendientes.ToString(), description = "requieren seguimiento" }
                },
                    chartData = new
                    {
                        categories = estados,
                        series = new[]
                        {
                        new { name = "Cantidad", data = cantidades }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = labelsDistribucion,
                        series = seriesDistribucion
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetClientesReservas(DateTime startDate, DateTime endDate)
            {
                // Obtener todas las reservas con información de clientes en el período especificado
                var reservasClientes = await _context.Reservas
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaReserva >= startDate && r.FechaReserva <= endDate)
                    .ToListAsync();

                // Contar clientes únicos
                var clientesUnicos = reservasClientes
                    .Select(r => r.IdCliente)
                    .Distinct()
                    .Count();

                // Agrupar por cliente para ver la distribución
                var reservasPorCliente = reservasClientes
                    .GroupBy(r => r.IdCliente)
                    .Select(g => new
                    {
                        IdCliente = g.Key,
                        NombreCliente = g.First().Nombre + " " + g.First().Apellido,
                        CantidadReservas = g.Count()
                    })
                    .OrderByDescending(c => c.CantidadReservas)
                    .Take(10)
                    .ToList();

                // Calcular promedio de reservas por cliente
                double promedioReservas = 0;
                if (clientesUnicos > 0)
                {
                    promedioReservas = Math.Round((double)reservasClientes.Count / clientesUnicos, 1);
                }

                // Calcular porcentaje de clientes que realizaron más de una reserva
                var clientesMultiplesReservas = reservasPorCliente.Count(c => c.CantidadReservas > 1);
                double porcentajeClientesMultiplesReservas = 0;
                if (clientesUnicos > 0)
                {
                    porcentajeClientesMultiplesReservas = Math.Round((double)clientesMultiplesReservas / clientesUnicos * 100, 1);
                }

                // Preparar datos para el gráfico principal
                var nombreClientes = reservasPorCliente.Select(c => c.NombreCliente).ToArray();
                var cantidadReservas = reservasPorCliente.Select(c => c.CantidadReservas).ToArray();

                // Preparar datos para gráfico secundario (distribución por cantidad de reservas)
                var distribucionReservas = new[] { 0, 0, 0, 0 }; // 1 reserva, 2 reservas, 3 reservas, 4+ reservas
                foreach (var cliente in reservasPorCliente)
                {
                    if (cliente.CantidadReservas == 1) distribucionReservas[0]++;
                    else if (cliente.CantidadReservas == 2) distribucionReservas[1]++;
                    else if (cliente.CantidadReservas == 3) distribucionReservas[2]++;
                    else distribucionReservas[3]++;
                }

                // Preparar datos para tabla
                var tablaDatos = reservasPorCliente.Select(c => new
                {
                    categoria = c.NombreCliente,
                    valor = $"{c.CantidadReservas} reservas"
                }).ToList<object>();

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Clientes Únicos", value = clientesUnicos.ToString(), description = "con reservas en el período" },
                    new { title = "Promedio Reservas", value = promedioReservas.ToString(), description = "por cliente" },
                    new { title = "Clientes Recurrentes", value = $"{porcentajeClientesMultiplesReservas}%", description = "con más de una reserva" }
                },
                    chartData = new
                    {
                        categories = nombreClientes,
                        series = new[]
                        {
                        new { name = "Reservas", data = cantidadReservas }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = new[] { "1 Reserva", "2 Reservas", "3 Reservas", "4+ Reservas" },
                        series = distribucionReservas
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetPromedioCliente(DateTime startDate, DateTime endDate)
            {
                // Obtener todas las reservas con información de clientes en el período especificado
                var reservasClientes = await _context.Reservas
                    .Include(r => r.Cliente)
                    .Where(r => r.FechaReserva >= startDate && r.FechaReserva <= endDate)
                    .ToListAsync();

                // Contar clientes únicos
                var clientesUnicos = reservasClientes
                    .Select(r => r.IdCliente)
                    .Distinct()
                    .Count();

                // Obtener distribución de reservas por cliente
                var reservasPorCliente = reservasClientes
                    .GroupBy(r => r.IdCliente)
                    .Select(g => new
                    {
                        IdCliente = g.Key,
                        NombreCliente = g.First().Nombre + " " + g.First().Apellido,
                        CantidadReservas = g.Count()
                    })
                    .OrderByDescending(c => c.CantidadReservas)
                    .ToList();

                // Calcular promedio de reservas por cliente
                double promedioReservas = 0;
                if (clientesUnicos > 0)
                {
                    promedioReservas = Math.Round((double)reservasClientes.Count / clientesUnicos, 1);
                }

                // Calcular mediana de reservas por cliente
                double medianaReservas = 0;
                var cantidades = reservasPorCliente.Select(c => c.CantidadReservas).OrderBy(c => c).ToList();
                if (cantidades.Count > 0)
                {
                    int mid = cantidades.Count / 2;
                    medianaReservas = cantidades.Count % 2 != 0 ? cantidades[mid] : (cantidades[mid - 1] + cantidades[mid]) / 2.0;
                }

                // Encontrar el cliente con más reservas
                var clienteMaxReservas = reservasPorCliente.FirstOrDefault();
                string clienteTop = clienteMaxReservas != null ? $"{clienteMaxReservas.NombreCliente}" : "Ninguno";
                string reservasTop = clienteMaxReservas != null ? $"{clienteMaxReservas.CantidadReservas}" : "0";

                // Preparar datos para el gráfico principal - Top 10 clientes
                var topClientes = reservasPorCliente.Take(10).ToList();
                var nombreClientes = topClientes.Select(c => c.NombreCliente).ToArray();
                var cantidadReservas = topClientes.Select(c => c.CantidadReservas).ToArray();

                // Preparar datos para gráfico secundario - Distribución de clientes por número de reservas
                var distribucion = new Dictionary<int, int>();
                foreach (var cliente in reservasPorCliente)
                {
                    if (!distribucion.ContainsKey(cliente.CantidadReservas))
                    {
                        distribucion[cliente.CantidadReservas] = 0;
                    }
                    distribucion[cliente.CantidadReservas]++;
                }

                var labelsDistribucion = distribucion.Keys.OrderBy(k => k).Select(k => k == 1 ? "1 Reserva" : $"{k} Reservas").ToArray();
                var seriesDistribucion = distribucion.OrderBy(k => k.Key).Select(k => k.Value).ToArray();

                // Preparar datos para tabla
                var tablaDatos = topClientes.Select(c => new
                {
                    categoria = c.NombreCliente,
                    valor = $"{c.CantidadReservas} reservas"
                }).ToList<object>();

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Promedio Reservas", value = promedioReservas.ToString(), description = "por cliente" },
                    new { title = "Mediana Reservas", value = medianaReservas.ToString(), description = "por cliente" },
                    new { title = "Cliente Top", value = clienteTop, description = $"{reservasTop} reservas" }
                },
                    chartData = new
                    {
                        categories = nombreClientes,
                        series = new[]
                        {
                        new { name = "Reservas", data = cantidadReservas }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = labelsDistribucion,
                        series = seriesDistribucion
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetIngresosPeriodo(DateTime startDate, DateTime endDate)
            {
                // Obtener todos los pagos en el período especificado
                var pagos = await _context.HistorialPagos
                    .Where(p => p.FechaPago >= startDate && p.FechaPago <= endDate)
                    .ToListAsync();

                // Agrupar por mes
                var pagosPorMes = pagos
                    .GroupBy(p => new { p.FechaPago.Year, p.FechaPago.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalMonto = g.Sum(p => p.Monto)
                    })
                    .OrderBy(g => g.Year)
                    .ThenBy(g => g.Month)
                    .ToList();

                // Calcular total de ingresos
                decimal totalIngresos = pagos.Sum(p => p.Monto);

                // Calcular promedio mensual
                decimal promedioMensual = 0;
                if (pagosPorMes.Count > 0)
                {
                    promedioMensual = Math.Round(totalIngresos / pagosPorMes.Count, 2);
                }

                // Calcular crecimiento
                string crecimiento = "0%";
                if (pagosPorMes.Count >= 2)
                {
                    var primerMes = pagosPorMes.First().TotalMonto;
                    var ultimoMes = pagosPorMes.Last().TotalMonto;
                    if (primerMes > 0)
                    {
                        var porcentaje = Math.Round((decimal)((ultimoMes / primerMes) - 1) * 100, 0);
                        crecimiento = $"{porcentaje}%";
                    }
                }

                // Preparar datos para el gráfico principal
                var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
                var datos = new decimal[12];

                foreach (var item in pagosPorMes)
                {
                    datos[item.Month - 1] = item.TotalMonto;
                }

                // Convertir datos a double para gráficos
                var datosDouble = datos.Select(d => (double)d).ToArray();

                // Preparar datos para gráfico secundario (por trimestre)
                var datosPorTrimestre = new[]
                {
                (double)(datos[0] + datos[1] + datos[2]),     // Q1
                (double)(datos[3] + datos[4] + datos[5]),     // Q2
                (double)(datos[6] + datos[7] + datos[8]),     // Q3
                (double)(datos[9] + datos[10] + datos[11])    // Q4
            };

                // Preparar datos para tabla
                var tablaDatos = new List<object>();
                for (int i = 0; i < 12; i++)
                {
                    if (datos[i] > 0)
                    {
                        tablaDatos.Add(new
                        {
                            categoria = meses[i],
                            valor = $"${datos[i]}"
                        });
                    }
                }

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Total Ingresos", value = $"${totalIngresos}", description = "en el período seleccionado" },
                    new { title = "Promedio Mensual", value = $"${promedioMensual}", description = "de ingresos" },
                    new { title = "Crecimiento", value = crecimiento, description = "vs. período anterior" }
                },
                    chartData = new
                    {
                        categories = meses,
                        series = new[]
                        {
                        new { name = "Ingresos", data = datosDouble }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = new[] { "Q1", "Q2", "Q3", "Q4" },
                        series = datosPorTrimestre
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetPagosPromedio(DateTime startDate, DateTime endDate)
            {
                // Obtener todos los pagos en el período especificado
                var pagos = await _context.HistorialPagos
                    .Where(p => p.FechaPago >= startDate && p.FechaPago <= endDate)
                    .ToListAsync();

                // Obtener el total de pagos
                int totalPagos = pagos.Count;

                // Calcular el monto total y promedio
                decimal montoTotal = pagos.Sum(p => p.Monto);
                decimal montoPromedio = totalPagos > 0 ? Math.Round(montoTotal / totalPagos, 2) : 0;

                // Calcular la mediana de pagos
                decimal medianaMontos = 0;
                var montos = pagos.Select(p => p.Monto).OrderBy(m => m).ToList();
                if (montos.Count > 0)
                {
                    int mid = montos.Count / 2;
                    medianaMontos = montos.Count % 2 != 0 ? montos[mid] : (montos[mid - 1] + montos[mid]) / 2;
                }

                // Agrupar por rango de montos para análisis
                var rangos = new[]
                {
                new { Min = 0M, Max = 100M, Nombre = "0-100" },
                new { Min = 100M, Max = 500M, Nombre = "100-500" },
                new { Min = 500M, Max = 1000M, Nombre = "500-1000" },
                new { Min = 1000M, Max = 2000M, Nombre = "1000-2000" },
                new { Min = 2000M, Max = decimal.MaxValue, Nombre = "2000+" }
            };

                var pagosPorRango = rangos.Select(r => new
                {
                    Rango = r.Nombre,
                    Cantidad = pagos.Count(p => p.Monto >= r.Min && p.Monto < r.Max),
                    MontoTotal = pagos.Where(p => p.Monto >= r.Min && p.Monto < r.Max).Sum(p => p.Monto)
                }).ToList();

                // Preparar datos para el gráfico principal
                var categorias = pagosPorRango.Select(r => r.Rango).ToArray();
                var cantidades = pagosPorRango.Select(r => r.Cantidad).ToArray();

                // Preparar datos para gráfico secundario (distribución por monto)
                var montosTotales = pagosPorRango.Select(r => (double)r.MontoTotal).ToArray();

                // Preparar datos para tabla
                var tablaDatos = pagosPorRango.Select(r => new
                {
                    categoria = $"${r.Rango}",
                    valor = $"{r.Cantidad} pagos (${r.MontoTotal})"
                }).ToList<object>();

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Total Pagos", value = totalPagos.ToString(), description = "en el período" },
                    new { title = "Monto Promedio", value = $"${montoPromedio}", description = "por pago" },
                    new { title = "Monto Mediana", value = $"${medianaMontos}", description = "valor medio" }
                },
                    chartData = new
                    {
                        categories = categorias,
                        series = new[]
                        {
                        new { name = "Cantidad", data = cantidades }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = categorias,
                        series = montosTotales
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetBobedasEstado()
            {
                // Obtener todas las bóvedas
                var bobedas = await _context.Bobedas.ToListAsync();

                // Agrupar por estado
                var estadoBobedas = bobedas
                    .GroupBy(b => b.Estado.ToLower())
                    .Select(g => new
                    {
                        Estado = g.Key,
                        Count = g.Count(),
                        Porcentaje = bobedas.Count > 0 ? Math.Round((double)g.Count() / bobedas.Count * 100, 1) : 0
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList();

                // Preparar datos para los KPIs
                int totalBobedas = bobedas.Count;
                int bobedasLibres = estadoBobedas.FirstOrDefault(e => e.Estado == "libre")?.Count ?? 0;
                int bobedasArrendadas = estadoBobedas.FirstOrDefault(e => e.Estado == "arriendo")?.Count ?? 0;
                int bobedasPropietarios = estadoBobedas.FirstOrDefault(e => e.Estado == "propietario")?.Count ?? 0;

                double porcentajeDisponibilidad = totalBobedas > 0 ? Math.Round((double)bobedasLibres / totalBobedas * 100, 1) : 0;
                string porcentajeDisponibilidadStr = $"{porcentajeDisponibilidad}%";

                // Preparar datos para el gráfico principal
                var estados = estadoBobedas.Select(e => FormatEstado(e.Estado)).ToArray();
                var cantidades = estadoBobedas.Select(e => e.Count).ToArray();

                // Preparar datos para gráfico secundario (distribución)
                var seriesDistribucion = estadoBobedas.Select(e => e.Count).ToArray();
                var labelsDistribucion = estadoBobedas.Select(e => FormatEstado(e.Estado)).ToArray();

                // Preparar datos para tabla
                var tablaDatos = estadoBobedas.Select(e => new
                {
                    categoria = FormatEstado(e.Estado),
                    valor = $"{e.Count} bóvedas ({e.Porcentaje}%)"
                }).ToList<object>();

                return new
                {
                    kpis = new[]
                    {
                    new { title = "Total Bóvedas", value = totalBobedas.ToString(), description = "registradas en el sistema" },
                    new { title = "Bóvedas Libres", value = bobedasLibres.ToString(), description = $"{porcentajeDisponibilidadStr} disponibles" },
                    new { title = "Bóvedas Ocupadas", value = (bobedasArrendadas + bobedasPropietarios).ToString(), description = "arrendadas o con propietario" }
                },
                    chartData = new
                    {
                        categories = estados,
                        series = new[]
                        {
                        new { name = "Cantidad", data = cantidades }
                    }
                    },
                    secondaryChart = new
                    {
                        labels = labelsDistribucion,
                        series = seriesDistribucion
                    },
                    tableData = tablaDatos
                };
            }

            private async Task<object> GetPrecioPromedio()
            {
                // Obtener todos los precios
                var precios = await _context.Precios
                    .Include(p => p.Sector)
                    .ToListAsync();

                // Calcular precio promedio general
                decimal precioPromedio = precios.Any() ? Math.Round(precios.Average(p => p.PrecioValor), 2) : 0;

                // Obtener precio mínimo y máximo
                decimal precioMinimo = precios.Any() ? precios.Min(p => p.PrecioValor) : 0;
                decimal precioMaximo = precios.Any() ? precios.Max(p => p.PrecioValor) : 0;

            // Agrupar por tipo de bóveda para obtener el precio promedio por tipo
            var preciosPorTipo = precios
                .GroupBy(p => p.PrecioValor)
                .Select(g => new
                {
                    Tipo = g.Key,
                    PrecioPromedio = Math.Round(g.Average(p => p.PrecioValor), 2),
                    Cantidad = g.Count()
                })
                .OrderByDescending(p => p.PrecioPromedio)
                .Take(10)
                .ToList();

            // Preparar datos para el gráfico principal
            var tiposBobeda = preciosPorTipo.Select(p => p.Tipo).ToArray();
            var preciosPromedio = preciosPorTipo.Select(p => (double)p.PrecioPromedio).ToArray();

            // Preparar datos para gráfico secundario (distribución de precios)
            var rangos = new[]
            {
    new { Min = 0M, Max = 100M, Nombre = "0-100" },
    new { Min = 100M, Max = 500M, Nombre = "100-500" },
    new { Min = 500M, Max = 1000M, Nombre = "500-1000" },
    new { Min = 1000M, Max = 2000M, Nombre = "1000-2000" },
    new { Min = 2000M, Max = decimal.MaxValue, Nombre = "2000+" }
};

            var distribucionPrecios = rangos.Select(r => new
            {
                Rango = r.Nombre,
                Cantidad = precios.Count(p => p.PrecioValor >= r.Min && p.PrecioValor < r.Max)
            }).ToList();

            var labelsDistribucion = distribucionPrecios.Select(d => d.Rango).ToArray();
            var seriesDistribucion = distribucionPrecios.Select(d => d.Cantidad).ToArray();

            // Preparar datos para tabla
            var tablaDatos = preciosPorTipo.Select(p => new
            {
                categoria = p.Tipo,
                valor = $"${p.PrecioPromedio} ({p.Cantidad} bóvedas)"
            }).ToList<object>();

            return new
            {
                kpis = new[]
                {
        new { title = "Precio Promedio", value = $"${precioPromedio}", description = "de todas las bóvedas" },
        new { title = "Precio Mínimo", value = $"${precioMinimo}", description = "bóveda más económica" },
        new { title = "Precio Máximo", value = $"${precioMaximo}", description = "bóveda premium" }
    },
                chartData = new
                {
                    categories = tiposBobeda,
                    series = new[]
                    {
            new { name = "Precio Promedio", data = preciosPromedio }
        }
                },
                secondaryChart = new
                {
                    labels = labelsDistribucion,
                    series = seriesDistribucion
                },
                tableData = tablaDatos
            };
        }

        private async Task<object> GetBobedaDemanda(DateTime startDate, DateTime endDate)
        {
            // Obtener todas las reservas con información de bóvedas en el período especificado
            var reservas = await _context.Reservas
                .Include(r => r.Precio)
                .ThenInclude(p => p.Sector)
                .Where(r => r.FechaReserva >= startDate && r.FechaReserva <= endDate)
                .ToListAsync();

            // Agrupar por tipo de bóveda para ver cuáles tienen más demanda
            var demandaPorTipo = reservas
                .GroupBy(r => r.Precio.PrecioValor)
                .Select(g => new
                {
                    Tipo = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = reservas.Count > 0 ? Math.Round((double)g.Count() / reservas.Count * 100, 1) : 0
                })
                .OrderByDescending(d => d.Cantidad)
                .Take(10)
                .ToList();

            // Obtener el total de reservas y tipos de bóveda reservados
            int totalReservas = reservas.Count;
            int tiposBobeda = demandaPorTipo.Count;

            // Encontrar el tipo de bóveda más demandado
            var tipoBobedaTop = demandaPorTipo.FirstOrDefault();
            string tipoBobedaTopNombre = tipoBobedaTop != null ? tipoBobedaTop.Tipo.ToString() : "Ninguno";
            string tipoBobedaTopCantidad = tipoBobedaTop != null ? $"{tipoBobedaTop.Cantidad} reservas" : "0";

            // Preparar datos para el gráfico principal
            var tipos = demandaPorTipo.Select(d => d.Tipo).ToArray();
            var cantidades = demandaPorTipo.Select(d => d.Cantidad).ToArray();

            // Preparar datos para gráfico secundario (porcentajes)
            var porcentajes = demandaPorTipo.Select(d => d.Porcentaje).ToArray();

            // Preparar datos para tabla
            var tablaDatos = demandaPorTipo.Select(d => new
            {
                categoria = d.Tipo,
                valor = $"{d.Cantidad} reservas ({d.Porcentaje}%)"
            }).ToList<object>();

            return new
            {
                kpis = new[]
                {
            new { title = "Total Reservas", value = totalReservas.ToString(), description = "en el período seleccionado" },
            new { title = "Tipos de Bóveda", value = tiposBobeda.ToString(), description = "con reservas registradas" },
            new { title = "Bóveda más Demandada", value = tipoBobedaTopNombre, description = tipoBobedaTopCantidad }
        },
                chartData = new
                {
                    categories = tipos,
                    series = new[]
                    {
                new { name = "Reservas", data = cantidades }
            }
                },
                secondaryChart = new
                {
                    labels = tipos,
                    series = porcentajes
                },
                tableData = tablaDatos
            };
        }

        private async Task<object> GetMensajesEstado(DateTime startDate, DateTime endDate)
        {
            // Obtener todos los mensajes en el período especificado
            var mensajes = await _context.Mensajes
                .Where(m => m.FechaCreacion >= startDate && m.FechaCreacion <= endDate)
                .ToListAsync();

            // Agrupar por estado (leído/no leído)
            var estadoMensajes = new[]
            {
        new
        {
            Estado = "Leídos",
            Count = mensajes.Count(m => m.Leido),
            Porcentaje = mensajes.Count > 0 ? Math.Round((double)mensajes.Count(m => m.Leido) / mensajes.Count * 100, 1) : 0
        },
        new
        {
            Estado = "No Leídos",
            Count = mensajes.Count(m => !m.Leido),
            Porcentaje = mensajes.Count > 0 ? Math.Round((double)mensajes.Count(m => !m.Leido) / mensajes.Count * 100, 1) : 0
        }
    };

            // Preparar datos para los KPIs
            int totalMensajes = mensajes.Count;
            int mensajesNoLeidos = estadoMensajes.FirstOrDefault(e => e.Estado == "No Leídos")?.Count ?? 0;
            double porcentajeLeidos = mensajes.Count > 0 ? Math.Round((double)mensajes.Count(m => m.Leido) / mensajes.Count * 100, 1) : 0;

            // Agrupar mensajes por día para ver tendencia
            var mensajesPorDia = mensajes
                .GroupBy(m => m.FechaCreacion.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Cantidad = g.Count()
                })
                .OrderBy(m => m.Fecha)
                .ToList();

            // Preparar datos para el gráfico principal
            var estados = estadoMensajes.Select(e => e.Estado).ToArray();
            var cantidades = estadoMensajes.Select(e => e.Count).ToArray();

            // Preparar datos para gráfico secundario (tendencia por día)
            var fechas = mensajesPorDia.Select(m => m.Fecha.ToString("dd/MM")).ToArray();
            var cantidadesPorDia = mensajesPorDia.Select(m => m.Cantidad).ToArray();

            // Preparar datos para tabla
            var tablaDatos = estadoMensajes.Select(e => new
            {
                categoria = e.Estado,
                valor = $"{e.Count} mensajes ({e.Porcentaje}%)"
            }).ToList<object>();

            return new
            {
                kpis = new[]
                {
            new { title = "Total Mensajes", value = totalMensajes.ToString(), description = "en el período seleccionado" },
            new { title = "Mensajes No Leídos", value = mensajesNoLeidos.ToString(), description = "requieren atención" },
            new { title = "Porcentaje Leídos", value = $"{porcentajeLeidos}%", description = "del total de mensajes" }
        },
                chartData = new
                {
                    categories = estados,
                    series = new[]
                    {
                new { name = "Cantidad", data = cantidades }
            }
                },
                secondaryChart = new
                {
                    labels = fechas,
                    series = cantidadesPorDia
                },
                tableData = tablaDatos
            };
        }

        private async Task<object> GetTendenciasTiempo(DateTime startDate, DateTime endDate)
        {
            // Obtener todas las reservas en el período especificado
            var reservas = await _context.Reservas
                .Where(r => r.FechaReserva >= startDate && r.FechaReserva <= endDate)
                .ToListAsync();

            // Agrupar por día
            var reservasPorDia = reservas
                .GroupBy(r => r.FechaReserva.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Cantidad = g.Count()
                })
                .OrderBy(r => r.Fecha)
                .ToList();

            // Calcular tendencia lineal simple
            double tendencia = 0;
            if (reservasPorDia.Count >= 2)
            {
                var primero = reservasPorDia.First();
                var ultimo = reservasPorDia.Last();
                var diasTranscurridos = (ultimo.Fecha - primero.Fecha).TotalDays;
                if (diasTranscurridos > 0)
                {
                    tendencia = (ultimo.Cantidad - primero.Cantidad) / diasTranscurridos;
                }
            }

            string tendenciaStr = tendencia > 0 ? "Creciente" : (tendencia < 0 ? "Decreciente" : "Estable");
            string tendenciaPorcentaje = Math.Abs(tendencia) < 0.01 ? "Estable" : $"{Math.Round(Math.Abs(tendencia) * 100, 1)}% diario";

            // Calcular promedio diario
            double promedioDiario = reservasPorDia.Any() ? Math.Round(reservasPorDia.Average(r => r.Cantidad), 1) : 0;

            // Preparar datos para el gráfico principal
            var fechas = reservasPorDia.Select(r => r.Fecha.ToString("dd/MM")).ToArray();
            var cantidades = reservasPorDia.Select(r => r.Cantidad).ToArray();

            // Agrupar por día de la semana para ver patrones
            var reservasPorDiaSemana = reservas
                .GroupBy(r => r.FechaReserva.DayOfWeek)
                .Select(g => new
                {
                    DiaSemana = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = reservas.Count > 0 ? Math.Round((double)g.Count() / reservas.Count * 100, 1) : 0
                })
                .OrderBy(r => r.DiaSemana)
                .ToList();

            // Mapear días de la semana a nombres en español
            var diasSemana = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var nombresDias = reservasPorDiaSemana.Select(r => diasSemana[(int)r.DiaSemana]).ToArray();
            var cantidadesPorDia = reservasPorDiaSemana.Select(r => r.Cantidad).ToArray();

            // Preparar datos para tabla
            var tablaDatos = reservasPorDiaSemana.Select(r => new
            {
                categoria = diasSemana[(int)r.DiaSemana],
                valor = $"{r.Cantidad} reservas ({r.Porcentaje}%)"
            }).ToList<object>();

            return new
            {
                kpis = new[]
                {
            new { title = "Tendencia", value = tendenciaStr, description = tendenciaPorcentaje },
            new { title = "Promedio Diario", value = promedioDiario.ToString(), description = "reservas por día" },
            new { title = "Total Días", value = reservasPorDia.Count.ToString(), description = "con actividad" }
        },
                chartData = new
                {
                    categories = fechas,
                    series = new[]
                    {
                new { name = "Reservas", data = cantidades }
            }
                },
                secondaryChart = new
                {
                    labels = nombresDias,
                    series = cantidadesPorDia
                },
                tableData = tablaDatos
            };
        }

        private async Task<object> GetMesesActividad(DateTime startDate, DateTime endDate)
        {
            // Asegurar que estamos analizando al menos un año completo
            var fechaInicio = new DateTime(startDate.Year, 1, 1);
            var fechaFin = new DateTime(endDate.Year, 12, 31);

            // Obtener todas las reservas en el período ampliado
            var reservas = await _context.Reservas
                .Where(r => r.FechaReserva >= fechaInicio && r.FechaReserva <= fechaFin)
                .ToListAsync();

            // Agrupar por mes
            var reservasPorMes = reservas
                .GroupBy(r => new { r.FechaReserva.Year, r.FechaReserva.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Cantidad = g.Count()
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToList();

            // Determinar los meses con más y menos actividad
            var mesMasActivo = reservasPorMes.OrderByDescending(r => r.Cantidad).FirstOrDefault();
            var mesMenosActivo = reservasPorMes.OrderBy(r => r.Cantidad).FirstOrDefault();

            string mesMasActivoNombre = mesMasActivo != null ?
                new DateTime(mesMasActivo.Year, mesMasActivo.Month, 1).ToString("MMMM yyyy") :
                "Ninguno";

            string mesMenosActivoNombre = mesMenosActivo != null ?
                new DateTime(mesMenosActivo.Year, mesMenosActivo.Month, 1).ToString("MMMM yyyy") :
                "Ninguno";

            // Calcular variación estacional
            double variacionEstacional = 0;
            if (mesMasActivo != null && mesMenosActivo != null && mesMenosActivo.Cantidad > 0)
            {
                variacionEstacional = Math.Round(((double)mesMasActivo.Cantidad / mesMenosActivo.Cantidad - 1) * 100, 0);
            }

            // Preparar datos para el gráfico principal
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };
            var datosAnioActual = new int[12];
            var datosAnioAnterior = new int[12];

            int anioActual = endDate.Year;
            int anioAnterior = anioActual - 1;

            foreach (var item in reservasPorMes)
            {
                if (item.Year == anioActual)
                {
                    datosAnioActual[item.Month - 1] = item.Cantidad;
                }
                else if (item.Year == anioAnterior)
                {
                    datosAnioAnterior[item.Month - 1] = item.Cantidad;
                }
            }

            // Calcular trimestres para gráfico secundario
            var trimestresActual = new[]
            {
        datosAnioActual[0] + datosAnioActual[1] + datosAnioActual[2],     // Q1
        datosAnioActual[3] + datosAnioActual[4] + datosAnioActual[5],     // Q2
        datosAnioActual[6] + datosAnioActual[7] + datosAnioActual[8],     // Q3
        datosAnioActual[9] + datosAnioActual[10] + datosAnioActual[11]    // Q4
    };

            // Preparar datos para tabla
            var tablaDatos = new List<object>();
            for (int i = 0; i < 12; i++)
            {
                tablaDatos.Add(new
                {
                    categoria = meses[i],
                    valor = $"{datosAnioActual[i]} reservas ({anioActual})"
                });
            }

            return new
            {
                kpis = new[]
                {
            new { title = "Mes Más Activo", value = mesMasActivoNombre, description = mesMasActivo != null ? $"{mesMasActivo.Cantidad} reservas" : "0" },
            new { title = "Mes Menos Activo", value = mesMenosActivoNombre, description = mesMenosActivo != null ? $"{mesMenosActivo.Cantidad} reservas" : "0" },
            new { title = "Variación Estacional", value = $"{variacionEstacional}%", description = "entre máximo y mínimo" }
        },
                chartData = new
                {
                    categories = meses,
                    series = new[]
                    {
                new { name = anioActual.ToString(), data = datosAnioActual },
                new { name = anioAnterior.ToString(), data = datosAnioAnterior }
            }
                },
                secondaryChart = new
                {
                    labels = new[] { "Q1", "Q2", "Q3", "Q4" },
                    series = trimestresActual
                },
                tableData = tablaDatos
            };
        }

        private string FormatEstado(string estado)
        {
            if (string.IsNullOrEmpty(estado)) return "Desconocido";

            // Convertir primera letra a mayúscula
            return char.ToUpper(estado[0]) + estado.Substring(1).ToLower();
        }
    }
}
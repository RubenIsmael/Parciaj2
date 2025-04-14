using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using San_Agustin_Final.Data;
using San_Agustin_Final.Models;

namespace San_Agustin_Final.Services.ChatBot
{
    public class DatabaseQueryService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetAvailableBovedasAsync()
        {
            try
            {
                var bovedasDisponibles = await _context.Bobedas
                    .Where(b => b.Estado == "Disponible")
                    .ToListAsync();

                if (!bovedasDisponibles.Any())
                {
                    return "Actualmente no hay bóvedas disponibles.";
                }

                var divisiones = bovedasDisponibles
                    .GroupBy(b => b.Division)
                    .Select(g => new { Division = g.Key, Cantidad = g.Count() })
                    .ToList();

                string resultado = $"Tenemos {bovedasDisponibles.Count} bóvedas disponibles distribuidas de la siguiente manera: ";

                foreach (var div in divisiones)
                {
                    resultado += $"{div.Cantidad} en la división {div.Division}, ";
                }

                return resultado.TrimEnd(' ', ',') + ".";
            }
            catch (Exception ex)
            {
                return $"Hubo un error al obtener la información de bóvedas disponibles: {ex.Message}";
            }
        }

        public async Task<string> GetPricesAsync()
        {
            try
            {
                var precios = await _context.Precios.ToListAsync();

                if (!precios.Any())
                {
                    return "No se encontró información de precios.";
                }

                string resultado = "Los precios de nuestras bóvedas son los siguientes: ";

                foreach (var precio in precios)
                {
                    resultado += $"Sector {precio.Sector}: ${precio.PrecioValor:N2}, ";
                }

                return resultado.TrimEnd(' ', ',') + ".";
            }
            catch (Exception ex)
            {
                return $"Hubo un error al obtener la información de precios: {ex.Message}";
            }
        }

        public async Task<string> GetPendingPaymentsAsync()
        {
            try
            {
                var reservasPendientes = await _context.Reservas
                    .Where(r => r.EstadoPago == "Pendiente")
                    .ToListAsync();

                if (!reservasPendientes.Any())
                {
                    return "No hay pagos pendientes registrados.";
                }

                return $"Hay {reservasPendientes.Count} reservas con pagos pendientes. Para obtener información específica, por favor acérquese a nuestras oficinas o contacte al administrador.";
            }
            catch (Exception ex)
            {
                return $"Hubo un error al obtener la información de pagos pendientes: {ex.Message}";
            }
        }

        public async Task<string> GetPaymentHistoryAsync()
        {
            try
            {
                var totalPagos = await _context.HistorialPagos.CountAsync();
                var montoTotal = await _context.HistorialPagos.SumAsync(h => h.Monto);
                var ultimoPago = await _context.HistorialPagos
                    .OrderByDescending(h => h.FechaPago)
                    .FirstOrDefaultAsync();

                if (totalPagos == 0)
                {
                    return "No hay historial de pagos registrados.";
                }

                string resultado = $"En total se han registrado {totalPagos} pagos, sumando un total de ${montoTotal:N2}. ";

                if (ultimoPago != null)
                {
                    resultado += $"El último pago fue realizado el {ultimoPago.FechaPago.ToString("dd/MM/yyyy")} por un monto de ${ultimoPago.Monto:N2}.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return $"Hubo un error al obtener el historial de pagos: {ex.Message}";
            }
        }

        public async Task<string> GetReservationInfoAsync()
        {
            try
            {
                var totalReservas = await _context.Reservas.CountAsync();
                var reservasRecientes = await _context.Reservas
                    .OrderByDescending(r => r.FechaReserva)
                    .Take(5)
                    .ToListAsync();

                if (totalReservas == 0)
                {
                    return "No hay reservas registradas en el sistema.";
                }

                string resultado = $"Tenemos un total de {totalReservas} reservas en el sistema. ";

                if (reservasRecientes.Any())
                {
                    resultado += "Para realizar una reserva, necesitará proporcionar su cédula, nombre completo y seleccionar la bóveda y el plan de pago deseado. Puede hacerlo visitando nuestras oficinas o contactando a un administrador.";
                }

                return resultado;
            }
            catch (Exception ex)
            {
                return $"Hubo un error al obtener la información de reservas: {ex.Message}";
            }
        }

        public async Task<string> GetClientsInfoAsync()
        {
            try
            {
                var totalClientes = await _context.Clientes.CountAsync();

                if (totalClientes == 0)
                {
                    return "No hay clientes registrados en el sistema.";
                }

                return $"Tenemos {totalClientes} clientes registrados en nuestro sistema. Para información específica sobre su cuenta, por favor inicie sesión en su perfil o contacte a un administrador.";
            }
            catch (Exception ex)
            {
                return $"Hubo un error al obtener la información de clientes: {ex.Message}";
            }
        }
    }
}
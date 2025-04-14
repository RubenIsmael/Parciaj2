using Microsoft.AspNetCore.Mvc;
using San_Agustin_Final.Data;
using San_Agustin_Final.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class GestionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public GestionController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var reservasPendientes = _context.Reservas
            .Where(r => r.EstadoPago == "pendiente")
            .ToList();
        return View(reservasPendientes);
    }

    [HttpPost]
    public async Task<IActionResult> BuscarReserva(string Cedula)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Precio.Sector)
            .Include(r => r.Precio.PrecioValor)
            .FirstOrDefaultAsync(r => r.Cliente.Clave == Cedula);

        if (reserva == null)
        {
            return Json(new { success = false, message = "Reserva no encontrada" });
        }

        return Json(new
        {
            success = true,
            nombre = reserva.Nombre,
            apellido = reserva.Apellido,
            cedula = reserva.Cliente.Clave,
            bobeda = reserva.Precio.Sector,
            estadoPago = reserva.EstadoPago,
            montoPendiente = reserva.Precio.PrecioValor
        });
    }

    [HttpPost]
    public async Task<IActionResult> ProcesarPago(
        string ReservaId,
        string MetodoPago,
        IFormFile Comprobante,
        string NumeroTarjeta = null,
        string FechaExpiracion = null,
        string CVV = null)
    {
        try
        {
            var reserva = await _context.Reservas.FindAsync(int.Parse(ReservaId));

            if (reserva == null)
                return BadRequest("Reserva no encontrada");

            // Crear carpeta para el cliente
            string clientFolder = Path.Combine(
                @"C:\Users\HpOne\Documents\San_agustin",
                $"{reserva.Nombre}_{DateTime.Now:yyyyMMdd}"
            );
            Directory.CreateDirectory(clientFolder);

            // Procesar comprobante si existe
            if (Comprobante != null && Comprobante.Length > 0)
            {
                string fileName = $"Comprobante_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(Comprobante.FileName)}";
                string filePath = Path.Combine(clientFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Comprobante.CopyToAsync(stream);
                }
            }

            // Actualizar estado de pago
            reserva.EstadoPago = "pagado";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pago procesado exitosamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al procesar pago: {ex.Message}");
        }
    }
}
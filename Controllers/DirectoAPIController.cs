using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using San_Agustin_Final.Data;
using San_Agustin_Final.Models; 
using System;
using System.Threading.Tasks;

namespace San_Agustin_Final.Controllers
{
    [Route("DirectoAPI")]
    [ApiController]
    public class DirectoAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context; 

        public DirectoAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("GuardarReserva")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarReserva([FromBody] Reserva reserva)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Datos de reserva inválidos" });
            }

            try
            {
                // Agregar la reserva a la base de datos
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Reserva guardada correctamente", id = reserva.Id });
            }
            catch (Exception ex)
            {
                // Registrar la excepción para depuración
                Console.WriteLine(ex.Message);
                return StatusCode(500, new { success = false, message = "Error al guardar la reserva" });
            }
        }
    }
}
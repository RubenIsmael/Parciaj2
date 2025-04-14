using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using San_Agustin_Final.Data;
using San_Agustin_Final.Models;

namespace San_Agustin_Final.Controllers
{
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservas.Include(r => r.Precio).Include(r => r.Cliente);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Precio)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }


        // GET: Reservas/Create
        public IActionResult Create()
        {
            var precios = _context.Precios.ToList();

            ViewBag.IdPrecio = new SelectList(precios, "Id", "Sector");
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Clave");

            return View();
        }

        [HttpPost]
        [Route("api/reservas/crear")]
        public async Task<IActionResult> CrearReserva([FromBody] Reserva reserva)
        {
            if (reserva == null || reserva.IdCliente == 0 || reserva.IdPrecio == 0)
            {
                return BadRequest(new { mensaje = "Datos inválidos. Verifica que todos los campos estén completos." });
            }

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Reserva guardada con éxito", idGenerado = reserva.Id });
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,Nombre,Apellido,IdPrecio,EstadoPago,FechaReserva")] Reserva reserva)
        {
            // Verificar valores para depuración
            Console.WriteLine($"IdCliente: {reserva.IdCliente}");
            Console.WriteLine($"Nombre: {reserva.Nombre}");
            Console.WriteLine($"Apellido: {reserva.Apellido}");
            Console.WriteLine($"IdPrecio: {reserva.IdPrecio}");
            Console.WriteLine($"EstadoPago: {reserva.EstadoPago}");
            Console.WriteLine($"FechaReserva: {reserva.FechaReserva}");

            if (ModelState.IsValid)
            {
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, recargamos las listas desplegables
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Clave", reserva.IdCliente);
            ViewBag.IdPrecio = new SelectList(_context.Precios, "Id", "Sector", reserva.IdPrecio);

            return View(reserva);
        }
        // GET: Reservas/GetClientePorCedula
        [HttpGet]
        public IActionResult GetClientePorCedula(int id)
        {
            var cliente = _context.Clientes.Find(id);
            if (cliente != null)
            {
                return Json(new { success = true, nombres = cliente.Nombres, apellidos = cliente.Apellidos });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public IActionResult GetPrecio(int id)
        {
            var precio = _context.Precios.FirstOrDefault(p => p.Id == id);
            if (precio == null)
                return NotFound();

            return Json(new { precioValor = precio.PrecioValor });
        }

        // GET: Reservas/GetPrecioPorId
        [HttpGet]
        public JsonResult ObtenerPrecioPorId(int id)
        {
            var precio = _context.Precios
                .Where(p => p.Id == id)
                .Select(p => p.PrecioValor) 
                .FirstOrDefault();

            return Json(precio);
        }

        [HttpGet]
        public IActionResult ObtenerDescripcionBobeda(string sector)
        {
            try
            {
                var tipoBobeda = _context.TiposBobeda.FirstOrDefault(t => t.Nombre.ToLower() == sector.ToLower());
                if (tipoBobeda != null)
                {
                    return Json(new { success = true, descripcion = tipoBobeda.Descripcion });
                }
                return Json(new { success = false, message = "Descripción no encontrada" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult ObtenerPrecio(string division)
        {
            try
            {
                var precio = _context.Precios.FirstOrDefault(p => p.Sector.ToLower() == division.ToLower());
                if (precio != null)
                {
                    return Json(new { success = true, idPrecio = precio.Id, precioValor = precio.PrecioValor });
                }
                return Json(new { success = false, message = "Precio no encontrado" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["IdPrecio"] = new SelectList(_context.Precios, "Id", "Sector", reserva.IdPrecio);
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Clave", reserva.IdCliente);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("IdCliente,Nombre,Apellido,IdPrecio,EstadoPago,FechaReserva")] Reserva reserva)
        {
            Console.WriteLine($"IdCliente: {reserva.IdCliente}");
            Console.WriteLine($"Nombre: {reserva.Nombre}");
            Console.WriteLine($"Apellido: {reserva.Apellido}");
            Console.WriteLine($"IdPrecio: {reserva.IdPrecio}");
            Console.WriteLine($"EstadoPago: {reserva.EstadoPago}");
            Console.WriteLine($"FechaReserva: {reserva.FechaReserva}");

            if (ModelState.IsValid)
            {
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, recargamos las listas desplegables
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "Id", "Clave", reserva.IdCliente);
            ViewBag.IdPrecio = new SelectList(_context.Precios, "Id", "Sector", reserva.IdPrecio);

            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Precio)
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }
    }
}
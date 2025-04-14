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
    public class HistorialPagoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HistorialPagoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HistorialPagoes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.HistorialPagos.Include(h => h.Reserva);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: HistorialPagoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialPago = await _context.HistorialPagos
                .Include(h => h.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (historialPago == null)
            {
                return NotFound();
            }

            return View(historialPago);
        }

        // GET: HistorialPagoes/Create
        public IActionResult Create()
        {
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "Id", "Apellido");
            return View();
        }

        // POST: HistorialPagoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdReserva,Monto,FechaPago")] HistorialPago historialPago)
        {
            if (ModelState.IsValid)
            {
                _context.Add(historialPago);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "Id", "Apellido", historialPago.IdReserva);
            return View(historialPago);
        }

        // GET: HistorialPagoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialPago = await _context.HistorialPagos.FindAsync(id);
            if (historialPago == null)
            {
                return NotFound();
            }
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "Id", "Apellido", historialPago.IdReserva);
            return View(historialPago);
        }

        // POST: HistorialPagoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdReserva,Monto,FechaPago")] HistorialPago historialPago)
        {
            if (id != historialPago.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(historialPago);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HistorialPagoExists(historialPago.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "Id", "Apellido", historialPago.IdReserva);
            return View(historialPago);
        }

        // GET: HistorialPagoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var historialPago = await _context.HistorialPagos
                .Include(h => h.Reserva)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (historialPago == null)
            {
                return NotFound();
            }

            return View(historialPago);
        }

        // POST: HistorialPagoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var historialPago = await _context.HistorialPagos.FindAsync(id);
            if (historialPago != null)
            {
                _context.HistorialPagos.Remove(historialPago);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HistorialPagoExists(int id)
        {
            return _context.HistorialPagos.Any(e => e.Id == id);
        }
    }
}

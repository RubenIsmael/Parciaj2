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
    public class BobedasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BobedasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bobedas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Bobedas.ToListAsync());
        }

        // GET: Bobedas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bobeda = await _context.Bobedas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bobeda == null)
            {
                return NotFound();
            }

            return View(bobeda);
        }

        // GET: Bobedas/Create
        public IActionResult Create()
        {
            // Obtener los tipos de bóveda desde la base de datos
            var tiposBobeda = _context.TiposBobeda.ToList(); // enlace tabla exista

            // Pasar los tipos de bóveda a la vista
            ViewBag.TiposBobeda = new SelectList(tiposBobeda, "Id", "Nombre");

            return View();
        }

        // POST: Bobedas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Division,Estado")] Bobeda bobeda)
        {
            if (ModelState.IsValid)
            {
                // Obtener el ID de la división desde el valor seleccionado en el formulario
                int divisionId;
                if (int.TryParse(bobeda.Division, out divisionId))
                {
                    var tipoBobeda = await _context.TiposBobeda.FindAsync(divisionId);
                    if (tipoBobeda != null)
                    {
                        bobeda.Division = tipoBobeda.Nombre; // Guardar el nombre
                    }
                }

                var existingDivision = await _context.Bobedas
                    .FirstOrDefaultAsync(b => b.Division.ToLower() == bobeda.Division.ToLower());

                if (existingDivision != null)
                {
                    ModelState.AddModelError("Division", "La división ya existe.");
                    return View(bobeda);
                }

                _context.Add(bobeda);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bobeda);
        }
        // GET: Bobedas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bobeda = await _context.Bobedas.FindAsync(id);
            if (bobeda == null)
            {
                return NotFound();
            }
            return View(bobeda);
        }

        // POST: Bobedas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Division,Estado")] Bobeda bobeda)
        {
            if (id != bobeda.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bobeda);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BobedaExists(bobeda.Id))
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
            return View(bobeda);
        }

        // GET: Bobedas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bobeda = await _context.Bobedas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bobeda == null)
            {
                return NotFound();
            }

            return View(bobeda);
        }

        // POST: Bobedas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bobeda = await _context.Bobedas.FindAsync(id);
            if (bobeda != null)
            {
                _context.Bobedas.Remove(bobeda);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BobedaExists(int id)
        {
            return _context.Bobedas.Any(e => e.Id == id);
        }
    }
}

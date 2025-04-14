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
    public class TipoBobedasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoBobedasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TipoBobedas
        public async Task<IActionResult> Index()
        {
            return View(await _context.TiposBobeda.ToListAsync());
        }

        // GET: TipoBobedas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoBobeda = await _context.TiposBobeda
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoBobeda == null)
            {
                return NotFound();
            }

            return View(tipoBobeda);
        }

        // GET: TipoBobedas/Create
        public IActionResult Create()
        {
            // Obtener los tipos de bóveda desde la base de datos
            var tiposBobeda = _context.TiposBobeda.ToList(); // Asumiendo que tienes un contexto _context

            // Pasar los tipos de bóveda a la vista
            ViewBag.TiposBobeda = new SelectList(tiposBobeda, "Id", "Nombre");

            return View();
        }

        // POST: TipoBobedas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] TipoBobeda tipoBobeda)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoBobeda);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoBobeda);
        }

        // GET: TipoBobedas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoBobeda = await _context.TiposBobeda.FindAsync(id);
            if (tipoBobeda == null)
            {
                return NotFound();
            }
            return View(tipoBobeda);
        }

        // POST: TipoBobedas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] TipoBobeda tipoBobeda)
        {
            if (id != tipoBobeda.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tipoBobeda);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoBobedaExists(tipoBobeda.Id))
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
            return View(tipoBobeda);
        }

        // GET: TipoBobedas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoBobeda = await _context.TiposBobeda
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoBobeda == null)
            {
                return NotFound();
            }

            return View(tipoBobeda);
        }

        // POST: TipoBobedas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoBobeda = await _context.TiposBobeda.FindAsync(id);
            if (tipoBobeda != null)
            {
                _context.TiposBobeda.Remove(tipoBobeda);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoBobedaExists(int id)
        {
            return _context.TiposBobeda.Any(e => e.Id == id);
        }
    }
}

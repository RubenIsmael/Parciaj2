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
    public class PreciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PreciosController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetPreciosSectores()
        {
            var precios = await _context.Precios
                .Select(p => new
                {
                    p.Id,
                    p.Sector,                   
                    p.PrecioValor
                })
                .ToListAsync();

            return Json(precios);
        }
        // GET: Precios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Precios.ToListAsync());
        }

        // GET: Precios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precio = await _context.Precios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (precio == null)
            {
                return NotFound();
            }

            return View(precio);
        }

        // GET: Precios/Create
        public IActionResult Create()
        {
            // Obtener los tipos de bóveda desde la base de datos
            var tiposBobeda = _context.TiposBobeda.ToList(); 

            // Pasar los tipos de bóveda a la vista
            ViewBag.Sectores = new SelectList(tiposBobeda, "Id", "Nombre");

            return View();
        }

        [HttpGet("/api/precios/buscar")]
        public IActionResult GetPrecioPorSector(string sector)
        {
            var precio = _context.Precios.FirstOrDefault(p => p.Sector == sector);
            if (precio == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                id = precio.Id,
                precioValor = precio.PrecioValor.ToString("0.00")
            });
        }

        // POST: Precios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Sector,PrecioValor")] Precio precio)
        {
            if (ModelState.IsValid)
            {
                // Obtener el ID de la división desde el valor seleccionado en el formulario
                int divisionId;
                if (int.TryParse(precio.Sector, out divisionId)) // <- Usamos precio en lugar de Precio
                {
                    var tipoBobeda = await _context.TiposBobeda.FindAsync(divisionId);
                    if (tipoBobeda != null)
                    {
                        precio.Sector = tipoBobeda.Nombre; // Guardar el nombre correctamente
                    }

                    // Declarar la variable existePrecio antes de usarla
                    var existePrecio = await _context.Precios.AnyAsync(p => p.Sector == precio.Sector);

                    if (existePrecio)
                    {
                        ModelState.AddModelError("Sector", "Este sector ya tiene un precio asignado.");
                    }
                    else
                    {
                        _context.Add(precio);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }


            // Si llega aquí, es porque algo falló, así que se vuelve a cargar la vista
            var tiposBobeda = _context.TiposBobeda.ToList();
            ViewBag.Sectores = new SelectList(tiposBobeda, "Id", "Nombre");

            return View(precio); // Asegura que el método siempre retorna una vista
        }

        // GET: Precios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precio = await _context.Precios.FindAsync(id);
            if (precio == null)
            {
                return NotFound();
            }
            return View(precio);
        }

        // POST: Precios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Sector,PrecioValor")] Precio precio)
        {
            if (id != precio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(precio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrecioExists(precio.Id))
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
            return View(precio);
        }

        // GET: Precios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var precio = await _context.Precios
                .FirstOrDefaultAsync(m => m.Id == id);
            if (precio == null)
            {
                return NotFound();
            }

            return View(precio);
        }

        // POST: Precios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var precio = await _context.Precios.FindAsync(id);
            if (precio != null)
            {
                _context.Precios.Remove(precio);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrecioExists(int id)
        {
            return _context.Precios.Any(e => e.Id == id);
        }
    }
}

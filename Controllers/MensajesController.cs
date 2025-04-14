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
    public class MensajesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MensajesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mensajes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mensajes.ToListAsync());
        }

        // GET: Mensajes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajes = await _context.Mensajes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mensajes == null)
            {
                return NotFound();
            }

            return View(mensajes);
        }

        // GET: Mensajes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mensajes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreCompleto,CorreoElectronico,Telefono,Mensaje,FechaCreacion")] Mensajes mensajes)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mensajes);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mensajes);
        }

        // GET: Mensajes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajes = await _context.Mensajes.FindAsync(id);
            if (mensajes == null)
            {
                return NotFound();
            }
            return View(mensajes);
        }

        // POST: Mensajes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreCompleto,CorreoElectronico,Telefono,Mensaje,FechaCreacion")] Mensajes mensajes)
        {
            if (id != mensajes.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mensajes);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MensajesExists(mensajes.Id))
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
            return View(mensajes);
        }

        // GET: Mensajes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mensajes = await _context.Mensajes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mensajes == null)
            {
                return NotFound();
            }

            return View(mensajes);
        }

        // POST: Mensajes/EnviarMensaje
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensaje([Bind("NombreCompleto,CorreoElectronico,Telefono,Mensaje")] Mensajes mensajes)
        {
            if (ModelState.IsValid)
            {
                mensajes.FechaCreacion = DateTime.Now; // Establecer la fecha de creación
                mensajes.Leido = false; // Asegúrate de establecer el estado de leído
                _context.Add(mensajes);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "Su mensaje fue enviado de forma correcta."; // Mensaje de confirmación
                return RedirectToAction("Contacto", "Home"); // Redirigir a la vista de Contacto
            }
            return View(mensajes); // Volver a mostrar el formulario con errores
        }

        // POST: Mensajes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mensajes = await _context.Mensajes.FindAsync(id);
            if (mensajes != null)
            {
                _context.Mensajes.Remove(mensajes);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MensajesExists(int id)
        {
            return _context.Mensajes.Any(e => e.Id == id);
        }
    }
}
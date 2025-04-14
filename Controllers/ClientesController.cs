using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using San_Agustin_Final.Data;
using San_Agustin_Final.Models;
using San_Agustin_Final.Validations;

namespace San_Agustin_Final.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombres,Apellidos,Clave,Correo")] Cliente cliente)
        {
            // Verificar si se reciben los valores correctos
            System.Diagnostics.Debug.WriteLine($"Nombres: {cliente.Nombres}");
            System.Diagnostics.Debug.WriteLine($"Apellidos: {cliente.Apellidos}");
            System.Diagnostics.Debug.WriteLine($"Clave: {cliente.Clave}");
            System.Diagnostics.Debug.WriteLine($"Correo: {cliente.Correo}");

            // Validar la cédula manualmente antes de verificar ModelState
            var cedulaValidator = new CedulaEcuatorianaAttribute();
            bool esValida = cedulaValidator.IsValid(cliente.Clave);
            System.Diagnostics.Debug.WriteLine($"¿Cédula válida?: {esValida}");

            if (!esValida)
            {
                ModelState.AddModelError("Clave", "La cédula ingresada no es válida.");
            }

            // Verificar si la cédula ya existe
            if (_context.Clientes.Any(c => c.Clave == cliente.Clave))
            {
                ModelState.AddModelError("Clave", "La cédula ya está registrada.");
            }

            // Verificar si el correo ya existe
            if (_context.Clientes.Any(c => c.Correo == cliente.Correo))
            {
                ModelState.AddModelError("Correo", "El correo electrónico ya está registrado.");
            }

            // Imprimir los errores de ModelState para depuración
            foreach (var state in ModelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en {state.Key}: {state.Value.Errors[0].ErrorMessage}");
                }
            }

            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState es válido, agregando cliente a la base de datos");
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ModelState NO es válido, no se agregó el cliente");
            }

            return View(cliente);
        }
        // POST: Clientes/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Cliente cliente)
        {
            // Validar la cédula manualmente
            var cedulaValidator = new CedulaEcuatorianaAttribute();
            bool esValida = cedulaValidator.IsValid(cliente.Clave);

            if (!esValida)
            {
                return Json(new { success = false, errors = new { Clave = "La cédula ingresada no es válida." } });
            }

            // Verificar si la cédula ya existe
            if (_context.Clientes.Any(c => c.Clave == cliente.Clave))
            {
                return Json(new { success = false, errors = new { Clave = "La cédula ya está registrada." } });
            }
            // Verificar si la cédula ya existe
            if (_context.Clientes.Any(c => c.Clave == cliente.Clave))
            {
                return Json(new { success = false, errors = new { Clave = "La cédula ya está registrada." } });
            }

            // Verificar si el correo ya existe
            if (_context.Clientes.Any(c => c.Correo == cliente.Correo))
            {
                return Json(new { success = false, errors = new { Correo = "El correo electrónico ya está registrado." } });
            }

            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                // Devolver el cliente recién creado, incluyendo su ID asignado por la base de datos
                return Json(new
                {
                    success = true,
                    message = "Cliente registrado con éxito",
                    cliente = new
                    {
                        id = cliente.Id,
                        clave = cliente.Clave,
                        nombres = cliente.Nombres,
                        apellidos = cliente.Apellidos
                    }
                });
            }

            // Si hay errores de validación en el ModelState
            var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).First()
                );

            return Json(new { success = false, errors = errors });
        }
        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombres,Apellidos,Clave,Correo")] Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return NotFound();
            }

            // Validar la cédula manualmente
            var cedulaValidator = new CedulaEcuatorianaAttribute();
            var validationResult = cedulaValidator.GetValidationResult(cliente.Clave, new ValidationContext(cliente, null, null));

            if (validationResult != ValidationResult.Success)
            {
                ModelState.AddModelError("Clave", validationResult.ErrorMessage);
            }

            // Verificar si la cédula ya existe (en otro registro)
            if (_context.Clientes.Any(c => c.Clave == cliente.Clave && c.Id != cliente.Id))
            {
                ModelState.AddModelError("Clave", "La cédula ya está registrada.");
            }

            // Verificar si el correo ya existe (en otro registro)
            if (_context.Clientes.Any(c => c.Correo == cliente.Correo && c.Id != cliente.Id))
            {
                ModelState.AddModelError("Correo", "El correo electrónico ya está registrado.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
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
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }

    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using San_Agustin_Final.Data;
using San_Agustin_Final.Models;
using Microsoft.EntityFrameworkCore;

namespace San_Agustin_Final.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Agregar el contexto de la base de datos

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context) // Inyectar el contexto
        {
            _logger = logger;
            _context = context; // Inicializar el contexto
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Quienes()
        {
            return View();
        }

        public IActionResult Contacto()
        {
            return View();
        }
        public IActionResult Gestion()
        {
            return View();
        }

        public IActionResult Aporte()
        {
            return View();
        }

        public async Task<IActionResult> Disponibilidad()
        {
            // Cargar las bóvedas
            var bobedas = await _context.Bobedas.ToListAsync();

            // Cargar los clientes para el dropdown
            ViewBag.Clientes = await _context.Clientes.ToListAsync();

            return View(bobedas);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]



        public IActionResult Privacy()
        {
            return View();
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
        [HttpPost]
        public IActionResult Create([FromBody] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Reservas.Add(reserva);
                    _context.SaveChanges();

                    // Actualizar estado de la bóveda
                    var bobeda = _context.Bobedas.Find(reserva.Id);
                    if (bobeda != null)
                    {
                        bobeda.Estado = "arriendo";
                        _context.SaveChanges();
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                }
            }
            return Json(new { success = false, message = "Datos de reserva inválidos" });
        }

        [HttpPost]
        public async Task<IActionResult> BuscarReserva(string Cedula)
        {
            if (string.IsNullOrEmpty(Cedula))
            {
                return Json(new { success = false, message = "Por favor ingrese un número de cédula" });
            }

            try
            {
                // Buscar el cliente por cédula (clave)
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Clave == Cedula);

                if (cliente == null)
                {
                    return Json(new { success = false, message = "No se encontró ningún cliente con esa cédula" });
                }

                // Obtener las reservas del cliente
                var reservas = await _context.Reservas
                    .Include(r => r.Cliente)
                    .Include(r => r.Precio)
                    .Where(r => r.IdCliente == cliente.Id)
                    .ToListAsync();

                if (reservas.Count == 0)
                {
                    return Json(new { success = false, message = "No se encontraron reservas para este cliente" });
                }

                var resultados = new List<object>();

                foreach (var reserva in reservas)
                {
                    // Obtener el sector de la tabla Precios
                    var precio = await _context.Precios.FindAsync(reserva.IdPrecio);

                    // Obtener la descripción de la tabla TipoBobeda
                    var tipoBobeda = await _context.TiposBobeda.FirstOrDefaultAsync(t => t.Nombre == precio.Sector);

                    resultados.Add(new
                    {
                        reservaId = reserva.Id,
                        cedula = cliente.Clave,
                        nombre = reserva.Nombre,
                        apellido = reserva.Apellido,
                        sector = precio.Sector,
                        precio = precio.PrecioValor,
                        estadoPago = reserva.EstadoPago,
                        descripcion = tipoBobeda?.Descripcion ?? "No disponible",
                        montoPendiente = reserva.EstadoPago == "pendiente" ? precio.PrecioValor : 0
                    });
                }

                return Json(new { success = true, reservas = resultados });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al buscar reservas: {ex.Message}" });
            }
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
        // Agregar este método al HomeController para procesar pagos
        [HttpPost]
        public async Task<IActionResult> ProcesarPago(int ReservaId, string MetodoPago, IFormFile Comprobante = null)
        {
            try
            {
                // Buscar la reserva
                var reserva = await _context.Reservas.FindAsync(ReservaId);

                if (reserva == null)
                {
                    return Json(new { success = false, message = "Reserva no encontrada" });
                }

                // Cambiar el estado de pago
                reserva.EstadoPago = "pagado";

                // Registrar en historial de pagos
                var precio = await _context.Precios.FindAsync(reserva.IdPrecio);

                var historialPago = new HistorialPago
                {
                    IdReserva = ReservaId,
                    Monto = precio.PrecioValor,
                    FechaPago = DateTime.Now
                };

                _context.HistorialPagos.Add(historialPago);

                // Si es transferencia o depósito, guardar el comprobante
                if ((MetodoPago == "transferencia" || MetodoPago == "deposito") && Comprobante != null)
                {
                    // Aquí iría el código para guardar el archivo
                    // Para este ejemplo solo verificamos que exista
                    if (Comprobante.Length > 0)
                    {
                        // Código para guardar el archivo en el servidor
                        // string fileName = Path.GetFileName(Comprobante.FileName);
                        // string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                        // using (var stream = new FileStream(filePath, FileMode.Create))
                        // {
                        //     await Comprobante.CopyToAsync(stream);
                        // }
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Pago procesado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al procesar el pago: {ex.Message}" });
            }
        }

        // Método para manejar el envío de mensajes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensaje([Bind("NombreCompleto,CorreoElectronico,Telefono,Mensaje")] Mensajes mensajes)
        {
            if (ModelState.IsValid)
            {
                mensajes.FechaCreacion = DateTime.Now; // Establecer la fecha de creación
                _context.Add(mensajes);
                await _context.SaveChangesAsync();

                // Redirigir a la vista de Contacto
                TempData["MensajeExito"] = "Mensaje enviado de forma correcta."; // Mensaje de confirmación
                return RedirectToAction("Contacto"); // Redirigir a la vista de Contacto
            }
            return View(mensajes); // Volver a mostrar el formulario con errores
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Route("DirectoAPI")]
        [ApiController]
        public class DirectoAPIController : ControllerBase
        {
            private readonly ApplicationDbContext _context; // Ajusta según el nombre de tu DbContext

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
}

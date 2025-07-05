using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using RendicionesPrimar.Models.ViewModels;
using System.Security.Claims;
using RendicionesPrimar.Services;

namespace RendicionesPrimar.Controllers
{
    [Authorize]
    public class RendicionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly RendicionService _rendicionService;

        public RendicionesController(ApplicationDbContext context, IWebHostEnvironment environment, RendicionService rendicionService)
        {
            _context = context;
            _environment = environment;
            _rendicionService = rendicionService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
            var usuario = await _context.Usuarios.FindAsync(userId);
            
            ViewBag.UserName = usuario?.Nombre ?? "Usuario";
            await ActualizarNotificacionesNoLeidas(userId);

            IQueryable<Rendicion> query = _context.Rendiciones
                .Include(r => r.Usuario)
                .Include(r => r.ArchivosAdjuntos);

            // Filtrar según el rol
            if (userRole == "empleado")
            {
                query = query.Where(r => r.UsuarioId == userId);
            }
            else if (userRole == "aprobador1")
            {
                query = query.Where(r => r.Estado == "pendiente" || r.Estado == "aprobado_2");
            }
            else if (userRole == "aprobador2")
            {
                query = query.Where(r => r.Estado == "aprobado_1");
            }

            var rendiciones = await query
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            ViewBag.UserRole = userRole;
            return View(rendiciones);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            var usuario = await _context.Usuarios.FindAsync(userId);
            ViewBag.UserName = usuario?.Nombre ?? "Usuario";
            await ActualizarNotificacionesNoLeidas(userId);

            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .Include(r => r.ArchivosAdjuntos)
                .Include(r => r.Aprobador1)
                .Include(r => r.Aprobador2)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rendicion == null)
            {
                return NotFound();
            }

            // Verificar permisos
            if (userRole == "empleado" && rendicion.UsuarioId != userId)
            {
                return Forbid();
            }

            // Determinar qué acciones puede realizar
            bool canApprove1 = userRole == "aprobador1" && rendicion.Estado == "pendiente";
            bool canApprove2 = userRole == "aprobador2" && rendicion.Estado == "aprobado_1";
            bool canMarkPaid = userRole == "aprobador1" && rendicion.Estado == "aprobado_2";
            bool canEdit = userRole == "empleado" && rendicion.Estado == "pendiente" && rendicion.UsuarioId == userId;

            var rendicionDetalleViewModel = new RendicionDetalleViewModel
            {
                Id = rendicion.Id,
                NumeroTicket = rendicion.NumeroTicket,
                Titulo = rendicion.Titulo,
                Descripcion = rendicion.Descripcion ?? string.Empty,
                MontoTotal = rendicion.MontoTotal,
                Estado = rendicion.Estado,
                FechaCreacion = rendicion.FechaCreacion,
                Nombre = rendicion.Nombre,
                Apellidos = rendicion.Apellidos,
                Rut = rendicion.Rut,
                Telefono = rendicion.Telefono,
                Cargo = rendicion.Cargo,
                Departamento = rendicion.Departamento,
                ComentariosAprobador = rendicion.ComentariosAprobador,
                Usuario = rendicion.Usuario,
                ArchivosAdjuntos = rendicion.ArchivosAdjuntos.ToList(),
                CanApprove1 = canApprove1,
                CanApprove2 = canApprove2,
                CanMarkPaid = canMarkPaid,
                CanEdit = canEdit,
                UserRole = userRole,
                TimelineEvents = new List<string>() // Puedes poblar esto si tienes lógica de eventos
            };

            return View(rendicionDetalleViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario no encontrado.";
                return RedirectToAction("Index", "Home");
            }

            // Verificar si el perfil está completo
            if (string.IsNullOrWhiteSpace(usuario.Telefono) ||
                string.IsNullOrWhiteSpace(usuario.Cargo) ||
                string.IsNullOrWhiteSpace(usuario.Departamento))
            {
                TempData["ErrorMessage"] = "Por favor, completa tu información de perfil (Teléfono, Cargo, Departamento) antes de crear una rendición.";
                // Asumiendo que el perfil del empleado se edita en Empleados/Perfil
                return RedirectToAction("Perfil", "Empleados");
            }

            var model = new CrearRendicionViewModel
            {
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Rut = usuario.Rut,
                Telefono = usuario.Telefono,
                Cargo = usuario.Cargo,
                Departamento = usuario.Departamento
            };

            ViewBag.UserName = usuario.Nombre;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearRendicionViewModel model)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var usuario = await _context.Usuarios.FindAsync(userId);

            if (usuario == null)
            {
                ModelState.AddModelError("", "No se pudo verificar la identidad del usuario.");
            }

            if (!ModelState.IsValid)
            {
                // Repoblar datos de solo lectura si el modelo no es válido
                var userForView = await _context.Usuarios.FindAsync(userId);
                if (userForView != null)
                {
                    model.Nombre = userForView.Nombre;
                    model.Apellidos = userForView.Apellidos;
                    model.Rut = userForView.Rut;
                    model.Telefono = userForView.Telefono;
                    model.Cargo = userForView.Cargo;
                    model.Departamento = userForView.Departamento;
                    ViewBag.UserName = userForView.Nombre;
                }
                return View(model);
            }

            var rendicion = await _rendicionService.CrearRendicionAsync(userId, model.Titulo, model.Descripcion, model.MontoTotal);

            if (model.Archivos != null && model.Archivos.Count > 0)
            {
                await ProcesarArchivosAdjuntos(rendicion.Id, model.Archivos);
            }

            TempData["SuccessMessage"] = $"Rendición {rendicion.NumeroTicket} creada exitosamente";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Aprobar(int id, string? comentarios)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
            
            if (userRole == "aprobador1")
            {
                return await Aprobar1(id, comentarios);
            }
            else if (userRole == "aprobador2")
            {
                return await Aprobar2(id, comentarios);
            }
            
            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> Rechazar(int id, string? comentarios)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
            
            if (userRole == "aprobador1")
            {
                return await Rechazar1(id, comentarios);
            }
            else if (userRole == "aprobador2")
            {
                return await Rechazar2(id, comentarios);
            }
            
            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> Aprobar1(int id, string? comentarios)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
                if (userRole != "aprobador1")
                {
                    TempData["ErrorMessage"] = "No tienes permisos para aprobar rendiciones";
                    return RedirectToAction("Detalle", new { id });
                }
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ok = await _rendicionService.AprobarPrimeraInstanciaAsync(id, userId, comentarios);
                if (!ok)
                {
                    TempData["ErrorMessage"] = "No se pudo aprobar la rendición";
                    return RedirectToAction("Detalle", new { id });
                }
                TempData["SuccessMessage"] = "Rendición aprobada en primera instancia";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al aprobar la rendición: {ex.Message}";
                return RedirectToAction("Detalle", new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Aprobar2(int id, string? comentarios)
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
                if (userRole != "aprobador2")
                {
                    TempData["ErrorMessage"] = "No tienes permisos para aprobar rendiciones";
                    return RedirectToAction("Detalle", new { id });
                }
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var ok = await _rendicionService.AprobarSegundaInstanciaAsync(id, userId, comentarios);
                if (!ok)
                {
                    TempData["ErrorMessage"] = "No se pudo aprobar la rendición";
                    return RedirectToAction("Detalle", new { id });
                }
                TempData["SuccessMessage"] = "Rendición aprobada finalmente";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al aprobar la rendición: {ex.Message}";
                return RedirectToAction("Detalle", new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarcarPagada(int id)
        {
            try
            {
                // Verificar que el usuario sea aprobador1
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
                if (userRole != "aprobador1")
                {
                    TempData["ErrorMessage"] = "No tienes permisos para marcar rendiciones como pagadas";
                    return RedirectToAction("Detalle", new { id });
                }

                var rendicion = await _context.Rendiciones
                    .Include(r => r.Usuario)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (rendicion == null)
                {
                    TempData["ErrorMessage"] = "Rendición no encontrada";
                    return RedirectToAction("Index");
                }

                if (rendicion.Estado != "aprobado_2")
                {
                    TempData["ErrorMessage"] = "La rendición debe estar aprobada finalmente para marcarla como pagada";
                    return RedirectToAction("Detalle", new { id });
                }

                rendicion.Estado = "pagado";
                rendicion.FechaPago = DateTime.Now;

                await _context.SaveChangesAsync();

                // Notificar al empleado
                await CrearNotificacionEmpleado(rendicion, $"Tu rendición {rendicion.NumeroTicket} ha sido marcada como pagada.");
                await CrearNotificacionPagado(rendicion);

                TempData["SuccessMessage"] = "Rendición marcada como pagada";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al marcar la rendición como pagada: {ex.Message}";
                return RedirectToAction("Detalle", new { id });
            }
        }

        public async Task<IActionResult> DescargarArchivo(int id)
        {
            var archivo = await _context.ArchivosAdjuntos
                .Include(a => a.Rendicion)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (archivo == null)
            {
                return NotFound();
            }

            // Verificar permisos
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            if (userRole == "empleado" && archivo.Rendicion != null && archivo.Rendicion.UsuarioId != userId)
            {
                return Forbid();
            }

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", archivo.RutaArchivo);
            
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "El archivo no se encuentra disponible";
                return RedirectToAction("Detalle", new { id = archivo.RendicionId });
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            
            return File(fileBytes, archivo.TipoArchivo, archivo.NombreArchivo);
        }

        private async Task<string> GenerarNumeroTicket()
        {
            string ticket;
            bool existe;
            
            do
            {
                var random = new Random();
                ticket = $"RND-{random.Next(100000, 999999)}";
                existe = await _context.Rendiciones.AnyAsync(r => r.NumeroTicket == ticket);
            }
            while (existe);
            
            return ticket;
        }

        private async Task ProcesarArchivosAdjuntos(int rendicionId, List<IFormFile> archivos)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{archivo.FileName}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await archivo.CopyToAsync(stream);
                    }

                    var archivoAdjunto = new ArchivoAdjunto
                    {
                        RendicionId = rendicionId,
                        NombreArchivo = archivo.FileName,
                        RutaArchivo = fileName,
                        TipoArchivo = archivo.ContentType,
                        TamanoArchivo = archivo.Length,
                        FechaSubida = DateTime.Now
                    };

                    _context.ArchivosAdjuntos.Add(archivoAdjunto);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task CrearNotificacionAprobacion(Rendicion rendicion, Usuario? usuario)
        {
            var supervisores = await _context.Usuarios
                .Where(a => a.Rol == "aprobador1")
                .ToListAsync();

            var nombreUsuario = usuario?.Nombre + " " + usuario?.Apellidos ?? "Usuario";
            foreach (var supervisor in supervisores)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = supervisor.Id, // id de la tabla aprobadores
                    RendicionId = rendicion.Id,
                    Mensaje = $"Nueva rendición {rendicion.NumeroTicket} de {nombreUsuario} requiere aprobación.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "supervisor"
                };
                _context.Notificaciones.Add(notificacion);
            }
            await _context.SaveChangesAsync();
        }

        private async Task CrearNotificacionSegundaAprobacion(Rendicion rendicion)
        {
            var gerente = await _context.Aprobadores
                .FirstOrDefaultAsync(a => a.Rol == "aprobador2");

            if (gerente != null)
            {
                var nombreUsuario = rendicion.Usuario?.Nombre + " " + rendicion.Usuario?.Apellidos ?? "Usuario";
                var notificacion = new Notificacion
                {
                    UsuarioId = gerente.Id, // id de la tabla aprobadores
                    RendicionId = rendicion.Id,
                    Mensaje = $"Rendición {rendicion.NumeroTicket} de {nombreUsuario} requiere tu aprobación final.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "gerente"
                };
                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CrearNotificacionParaPago(Rendicion rendicion)
        {
            var supervisor = await _context.Aprobadores
                .FirstOrDefaultAsync(a => a.Rol == "aprobador1");

            if (supervisor != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = supervisor.Id, // id de la tabla aprobadores
                    RendicionId = rendicion.Id,
                    Mensaje = $"Rendición {rendicion.NumeroTicket} aprobada finalmente. Proceder con el pago.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "supervisor"
                };
                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CrearNotificacionPagado(Rendicion rendicion)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = rendicion.UsuarioId,
                RendicionId = rendicion.Id,
                Mensaje = $"Su rendición {rendicion.NumeroTicket} ha sido pagada exitosamente.",
                Leido = false,
                FechaCreacion = DateTime.Now,
                TipoRol = "empleado"
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
        }

        private async Task CrearNotificacionEmpleado(Rendicion rendicion, string mensaje)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = rendicion.UsuarioId, // id del empleado (tabla usuarios)
                RendicionId = rendicion.Id,
                Mensaje = mensaje,
                Leido = false,
                FechaCreacion = DateTime.Now,
                TipoRol = "empleado"
            };
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Rechazar1(int id, string? comentarios)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
            if (userRole != "aprobador1")
            {
                TempData["ErrorMessage"] = "No tienes permisos para rechazar rendiciones";
                return RedirectToAction("Detalle", new { id });
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ok = await _rendicionService.RechazarAsync(id, comentarios ?? "Rechazada por el supervisor", userId);
            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo rechazar la rendición";
                return RedirectToAction("Detalle", new { id });
            }
            TempData["SuccessMessage"] = "Rendición rechazada";
            return RedirectToAction("Detalle", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Rechazar2(int id, string? comentarios)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
            if (userRole != "aprobador2")
            {
                TempData["ErrorMessage"] = "No tienes permisos para rechazar rendiciones";
                return RedirectToAction("Detalle", new { id });
            }
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ok = await _rendicionService.RechazarAsync(id, comentarios ?? "Rechazada por el gerente", userId);
            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo rechazar la rendición";
                return RedirectToAction("Detalle", new { id });
            }
            TempData["SuccessMessage"] = "Rendición rechazada";
            return RedirectToAction("Detalle", new { id });
        }

        private async Task ActualizarNotificacionesNoLeidas(int userId)
        {
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido)
                .CountAsync();
            
            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var rendicion = await _context.Rendiciones.FindAsync(id);
            if (rendicion == null) return NotFound();

            var model = new CrearRendicionViewModel
            {
                Titulo = rendicion.Titulo,
                Descripcion = rendicion.Descripcion,
                MontoTotal = rendicion.MontoTotal
            };
            
            ViewBag.RendicionId = id;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, CrearRendicionViewModel model)
        {
            if (id <= 0) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.RendicionId = id;
                return View(model);
            }

            var rendicion = await _context.Rendiciones.FindAsync(id);
            if (rendicion == null) return NotFound();

            rendicion.Titulo = model.Titulo;
            rendicion.Descripcion = model.Descripcion;
            rendicion.MontoTotal = model.MontoTotal;
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Rendición actualizada exitosamente.";
            return RedirectToAction("Detalle", new { id = id });
        }
    }
}
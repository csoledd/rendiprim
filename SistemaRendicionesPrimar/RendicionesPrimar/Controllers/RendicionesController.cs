using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using RendicionesPrimar.Models.ViewModels;
using System.Security.Claims;

namespace RendicionesPrimar.Controllers
{
    [Authorize]
    public class RendicionesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RendicionesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Método para obtener el nombre del usuario buscando en ambas tablas
        private async Task<string> ObtenerNombreUsuario(int userId)
        {
            // Primero buscar en la tabla usuarios
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario != null)
            {
                return usuario.Nombre;
            }

            // Si no se encuentra en usuarios, buscar en aprobadores
            var aprobador = await _context.Set<Usuario>().FromSqlRaw("SELECT * FROM aprobadores WHERE id = {0}", userId).FirstOrDefaultAsync();
            return aprobador?.Nombre ?? "Usuario";
        }

        // Método para obtener los datos completos del usuario buscando en ambas tablas
        private async Task<Usuario?> ObtenerUsuarioCompleto(int userId)
        {
            // Primero buscar en la tabla usuarios
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario != null)
            {
                return usuario;
            }

            // Si no se encuentra en usuarios, buscar en aprobadores
            var aprobador = await _context.Set<Usuario>().FromSqlRaw("SELECT * FROM aprobadores WHERE id = {0}", userId).FirstOrDefaultAsync();
            return aprobador;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Obtener el usuario para el nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;
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

            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;
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
                NombreCompleto = rendicion.NombreCompleto,
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
            if (!User.IsInRole("empleado"))
            {
                return Forbid();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;
            await ActualizarNotificacionesNoLeidas(userId);

            // Obtener datos del usuario para prellenar el formulario
            var usuario = await ObtenerUsuarioCompleto(userId);

            // Prellenar datos personales si existen
            var model = new CrearRendicionViewModel
            {
                NombreCompleto = usuario?.NombreCompleto ?? string.Empty,
                Rut = usuario?.Rut ?? string.Empty,
                Telefono = usuario?.Telefono,
                Cargo = usuario?.Cargo,
                Departamento = usuario?.Departamento
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(CrearRendicionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var nombreUsuario = await ObtenerNombreUsuario(userId);
                ViewBag.UserName = nombreUsuario;
                await ActualizarNotificacionesNoLeidas(userId);
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var usuario = await ObtenerUsuarioCompleto(userId);

                var rendicion = new Rendicion
                {
                    Titulo = model.Titulo,
                    Descripcion = model.Descripcion,
                    MontoTotal = model.MontoTotal,
                    UsuarioId = userId,
                    Estado = "pendiente",
                    FechaCreacion = DateTime.Now,
                    NumeroTicket = await GenerarNumeroTicket(),
                    // Información personal
                    NombreCompleto = model.NombreCompleto,
                    Rut = model.Rut,
                    Telefono = model.Telefono,
                    Cargo = model.Cargo,
                    Departamento = model.Departamento
                };

                _context.Rendiciones.Add(rendicion);
                await _context.SaveChangesAsync();

                // Procesar archivos adjuntos
                if (model.Archivos != null && model.Archivos.Count > 0)
                {
                    await ProcesarArchivosAdjuntos(rendicion.Id, model.Archivos);
                }

                // Crear notificación para el aprobador
                await CrearNotificacionAprobacion(rendicion, usuario);

                TempData["SuccessMessage"] = $"Rendición {rendicion.NumeroTicket} creada exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear la rendición: {ex.Message}";
                return View(model);
            }
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
                // Verificar que el usuario sea aprobador1
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
                if (userRole != "aprobador1")
                {
                    TempData["ErrorMessage"] = "No tienes permisos para aprobar rendiciones";
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

                if (rendicion.Estado != "pendiente")
                {
                    TempData["ErrorMessage"] = "La rendición no está en estado pendiente";
                    return RedirectToAction("Detalle", new { id });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                rendicion.Estado = "aprobado_1";
                rendicion.FechaAprobacion1 = DateTime.Now;
                rendicion.Aprobador1Id = userId;
                
                if (!string.IsNullOrEmpty(comentarios))
                {
                    rendicion.ComentariosAprobador = comentarios;
                }

                await _context.SaveChangesAsync();

                // Notificar al segundo aprobador
                await CrearNotificacionSegundaAprobacion(rendicion);

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
                // Verificar que el usuario sea aprobador2
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
                if (userRole != "aprobador2")
                {
                    TempData["ErrorMessage"] = "No tienes permisos para aprobar rendiciones";
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

                if (rendicion.Estado != "aprobado_1")
                {
                    TempData["ErrorMessage"] = "La rendición debe estar aprobada en primera instancia";
                    return RedirectToAction("Detalle", new { id });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                rendicion.Estado = "aprobado_2";
                rendicion.FechaAprobacion2 = DateTime.Now;
                rendicion.Aprobador2Id = userId;
                
                if (!string.IsNullOrEmpty(comentarios))
                {
                    var comentarioExistente = rendicion.ComentariosAprobador ?? "";
                    rendicion.ComentariosAprobador = comentarioExistente + $"\nAprobación final: {comentarios}";
                }

                await _context.SaveChangesAsync();

                // Notificar al primer aprobador para pago
                await CrearNotificacionParaPago(rendicion);

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
            var aprobador = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Rol == "aprobador1");

            if (aprobador != null)
            {
                var nombreUsuario = usuario?.Nombre ?? "Usuario";
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Nueva rendición {rendicion.NumeroTicket} de {nombreUsuario} requiere aprobación.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "supervisor"
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();
            }
        }

        private async Task CrearNotificacionSegundaAprobacion(Rendicion rendicion)
        {
            var aprobador = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Rol == "aprobador2");

            if (aprobador != null)
            {
                var nombreUsuario = rendicion.Usuario?.Nombre ?? "Usuario";
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador.Id,
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
            var aprobador = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Rol == "aprobador1");

            if (aprobador != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador.Id,
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

        [HttpPost]
        public async Task<IActionResult> Rechazar1(int id, string? comentarios)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rendicion == null || rendicion.Estado != "pendiente")
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            rendicion.Estado = "rechazado";
            rendicion.FechaAprobacion1 = DateTime.Now;
            
            if (!string.IsNullOrEmpty(comentarios))
            {
                rendicion.ComentariosAprobador = $"Rechazado en primera revisión: {comentarios}";
            }
            else
            {
                rendicion.ComentariosAprobador = "Rechazado en primera revisión";
            }

            await _context.SaveChangesAsync();

            // Notificar al empleado que fue rechazado
            await CrearNotificacionRechazo(rendicion, "primera revisión");

            TempData["SuccessMessage"] = "Rendición rechazada";
            return RedirectToAction("Detalle", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Rechazar2(int id, string? comentarios)
        {
            try
            {
                // Verificar que el usuario sea aprobador2
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
                if (userRole != "aprobador2")
                {
                    TempData["ErrorMessage"] = "No tienes permisos para rechazar rendiciones";
                    return RedirectToAction("Detalle", new { id });
                }

                // Validar que se proporcione un motivo
                if (string.IsNullOrWhiteSpace(comentarios))
                {
                    TempData["ErrorMessage"] = "Debe proporcionar un motivo para el rechazo";
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

                if (rendicion.Estado != "aprobado_1")
                {
                    TempData["ErrorMessage"] = "La rendición debe estar aprobada en primera instancia para poder rechazarla";
                    return RedirectToAction("Detalle", new { id });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                rendicion.Estado = "rechazado";
                rendicion.FechaAprobacion2 = DateTime.Now;
                rendicion.Aprobador2Id = userId;
                
                var comentarioExistente = rendicion.ComentariosAprobador ?? "";
                rendicion.ComentariosAprobador = comentarioExistente + $"\n\n❌ RECHAZADO EN APROBACIÓN FINAL:\n{comentarios}";

                await _context.SaveChangesAsync();

                // Notificar al empleado que fue rechazado
                await CrearNotificacionRechazo(rendicion, "aprobación final");

                TempData["SuccessMessage"] = "Rendición rechazada exitosamente";
                return RedirectToAction("Detalle", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al rechazar la rendición: {ex.Message}";
                return RedirectToAction("Detalle", new { id });
            }
        }

        private async Task CrearNotificacionRechazo(Rendicion rendicion, string etapaRechazo)
        {
            var mensaje = $"Su rendición {rendicion.NumeroTicket} ha sido rechazada en {etapaRechazo}.";
            
            // Si hay comentarios del aprobador, incluirlos en la notificación
            if (!string.IsNullOrEmpty(rendicion.ComentariosAprobador))
            {
                var comentarios = rendicion.ComentariosAprobador;
                if (comentarios.Contains("❌ RECHAZADO EN APROBACIÓN FINAL:"))
                {
                    var motivoRechazo = comentarios.Split("❌ RECHAZADO EN APROBACIÓN FINAL:").Last().Trim();
                    mensaje += $"\n\nMotivo: {motivoRechazo}";
                }
                else if (comentarios.Contains("Rechazado en primera revisión:"))
                {
                    var motivoRechazo = comentarios.Split("Rechazado en primera revisión:").Last().Trim();
                    mensaje += $"\n\nMotivo: {motivoRechazo}";
                }
            }

            var notificacion = new Notificacion
            {
                UsuarioId = rendicion.UsuarioId,
                RendicionId = rendicion.Id,
                Mensaje = mensaje,
                Leido = false,
                FechaCreacion = DateTime.Now,
                TipoRol = "empleado"
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();
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
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "empleado";
            var rendicion = await _context.Rendiciones.FindAsync(id);
            if (rendicion == null)
            {
                return NotFound();
            }
            if (userRole != "empleado" || rendicion.UsuarioId != userId || rendicion.Estado != "pendiente")
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar esta rendición.";
                return RedirectToAction("Detalle", new { id });
            }
            var model = new CrearRendicionViewModel
            {
                Titulo = rendicion.Titulo,
                Descripcion = rendicion.Descripcion ?? string.Empty,
                MontoTotal = rendicion.MontoTotal,
                NombreCompleto = rendicion.NombreCompleto,
                Rut = rendicion.Rut,
                Telefono = rendicion.Telefono,
                Cargo = rendicion.Cargo,
                Departamento = rendicion.Departamento
            };
            ViewBag.UserName = User.Identity?.Name ?? "Usuario";
            return View("Crear", model);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, CrearRendicionViewModel model)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "empleado";
            var rendicion = await _context.Rendiciones.FindAsync(id);
            if (rendicion == null)
            {
                return NotFound();
            }
            if (userRole != "empleado" || rendicion.UsuarioId != userId || rendicion.Estado != "pendiente")
            {
                TempData["ErrorMessage"] = "No tienes permisos para editar esta rendición.";
                return RedirectToAction("Detalle", new { id });
            }
            if (!ModelState.IsValid)
            {
                ViewBag.UserName = User.Identity?.Name ?? "Usuario";
                return View("Crear", model);
            }
            rendicion.Titulo = model.Titulo;
            rendicion.Descripcion = model.Descripcion;
            rendicion.MontoTotal = model.MontoTotal;
            rendicion.NombreCompleto = model.NombreCompleto;
            rendicion.Rut = model.Rut;
            rendicion.Telefono = model.Telefono;
            rendicion.Cargo = model.Cargo;
            rendicion.Departamento = model.Departamento;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rendición editada exitosamente.";
            return RedirectToAction("Detalle", new { id });
        }
    }
}
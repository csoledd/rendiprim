using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using RendicionesPrimar.Models.ViewModels;
using RendicionesPrimar.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RendicionesPrimar.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AdminController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

        // Verificar que el usuario sea Camila (aprobador1)
        private async Task<bool> EsCamila()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";
            
            if (userRole != "aprobador1")
                return false;

            // Primero buscar en la tabla usuarios
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario != null)
            {
                return usuario.Email == "camila.flores@primar.cl";
            }

            // Si no se encuentra en usuarios, buscar en aprobadores
            var aprobador = await _context.Set<Usuario>().FromSqlRaw("SELECT * FROM aprobadores WHERE id = {0}", userId).FirstOrDefaultAsync();
            return aprobador?.Email == "camila.flores@primar.cl";
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<IActionResult> Index()
        {
            if (!await EsCamila())
            {
                return Forbid();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;
            await ActualizarNotificacionesNoLeidas(userId);

            var usuarios = await _context.Usuarios
                .Where(u => u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        public async Task<IActionResult> CrearUsuario()
        {
            if (!await EsCamila())
            {
                return Forbid();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;
            await ActualizarNotificacionesNoLeidas(userId);

            return View(new CrearUsuarioViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
        {
            if (!await EsCamila())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var nombreUsuario = await ObtenerNombreUsuario(userId);
                ViewBag.UserName = nombreUsuario;
                await ActualizarNotificacionesNoLeidas(userId);
                return View(model);
            }

            // Verificar si el email ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "El email ya está registrado");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var nombreUsuario = await ObtenerNombreUsuario(userId);
                ViewBag.UserName = nombreUsuario;
                await ActualizarNotificacionesNoLeidas(userId);
                return View(model);
            }

            // Crear hash de la contraseña
            var passwordHash = HashPassword(model.Password);

            var nuevoUsuario = new Usuario
            {
                Nombre = model.Nombre,
                NombreCompleto = model.NombreCompleto,
                Rut = model.Rut,
                Email = model.Email,
                Telefono = model.Telefono,
                PasswordHash = passwordHash,
                Rol = model.Rol,
                Cargo = model.Cargo,
                Departamento = model.Departamento,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Enviar email con credenciales
            await EnviarEmailCredenciales(nuevoUsuario, model.Password);

            TempData["SuccessMessage"] = $"Usuario {model.Nombre} creado exitosamente. Se han enviado las credenciales a su correo electrónico.";
            return RedirectToAction("Index");
        }

        private async Task EnviarEmailCredenciales(Usuario usuario, string password)
        {
            try
            {
                var asunto = "Credenciales de Acceso - Sistema de Rendiciones Primar";
                var mensaje = $@"
                    <h3>¡Bienvenido al Sistema de Rendiciones Primar!</h3>
                    <p>Hola <strong>{usuario.NombreCompleto}</strong>,</p>
                    <p>Tu cuenta ha sido creada exitosamente en el Sistema de Rendiciones Primar.</p>
                    
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                        <h4 style='color: #495057; margin-top: 0;'>Tus credenciales de acceso:</h4>
                        <p><strong>Email:</strong> {usuario.Email}</p>
                        <p><strong>Contraseña:</strong> {password}</p>
                        <p><strong>Rol:</strong> {ObtenerNombreRol(usuario.Rol)}</p>
                        <p><strong>Cargo:</strong> {usuario.Cargo ?? "No especificado"}</p>
                        <p><strong>Departamento:</strong> {usuario.Departamento ?? "No especificado"}</p>
                    </div>
                    
                    <div style='background-color: #e7f3ff; padding: 15px; border-radius: 8px; border-left: 4px solid #007bff;'>
                        <h4 style='color: #0056b3; margin-top: 0;'>Instrucciones importantes:</h4>
                        <ul style='margin: 10px 0; padding-left: 20px;'>
                            <li>Cambia tu contraseña en tu primer ingreso por seguridad</li>
                            <li>Mantén tus credenciales en un lugar seguro</li>
                            <li>Si olvidas tu contraseña, usa la opción '¿Olvidaste tu contraseña?'</li>
                        </ul>
                    </div>
                    
                    <p style='margin-top: 20px;'>
                        <a href='{Request.Scheme}://{Request.Host}' 
                           style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;'>
                            Acceder al Sistema
                        </a>
                    </p>
                    
                    <p style='color: #6c757d; font-size: 14px; margin-top: 30px;'>
                        Si tienes alguna pregunta, contacta al administrador del sistema.
                    </p>";

                await _emailService.EnviarNotificacionAsync(usuario.Email, asunto, mensaje);
            }
            catch (Exception ex)
            {
                // Log del error pero no fallar la creación del usuario
                // En un entorno de producción, podrías usar un logger real
                Console.WriteLine($"Error enviando email de credenciales: {ex.Message}");
            }
        }

        private string ObtenerNombreRol(string rol)
        {
            return rol switch
            {
                "empleado" => "Empleado",
                "aprobador1" => "Supervisor",
                "aprobador2" => "Gerente",
                "admin" => "Administrador",
                _ => rol
            };
        }

        public async Task<IActionResult> EditarUsuario(int id)
        {
            if (!await EsCamila())
            {
                return Forbid();
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;
            await ActualizarNotificacionesNoLeidas(userId);

            var usuarioEditar = await _context.Usuarios.FindAsync(id);
            if (usuarioEditar == null)
            {
                return NotFound();
            }

            var viewModel = new EditarUsuarioViewModel
            {
                Id = usuarioEditar.Id,
                Nombre = usuarioEditar.Nombre,
                NombreCompleto = usuarioEditar.NombreCompleto,
                Rut = usuarioEditar.Rut,
                Email = usuarioEditar.Email,
                Telefono = usuarioEditar.Telefono,
                Rol = usuarioEditar.Rol,
                Cargo = usuarioEditar.Cargo,
                Departamento = usuarioEditar.Departamento,
                Activo = usuarioEditar.Activo
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioViewModel model)
        {
            if (!await EsCamila())
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var nombreUsuario = await ObtenerNombreUsuario(userId);
                ViewBag.UserName = nombreUsuario;
                await ActualizarNotificacionesNoLeidas(userId);
                return View(model);
            }

            var usuarioEditar = await _context.Usuarios.FindAsync(model.Id);
            if (usuarioEditar == null)
            {
                return NotFound();
            }

            // Verificar si el email ya existe en otro usuario
            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
            {
                ModelState.AddModelError("Email", "El email ya está registrado por otro usuario");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var nombreUsuario = await ObtenerNombreUsuario(userId);
                ViewBag.UserName = nombreUsuario;
                await ActualizarNotificacionesNoLeidas(userId);
                return View(model);
            }

            // Actualizar datos
            usuarioEditar.Nombre = model.Nombre;
            usuarioEditar.NombreCompleto = model.NombreCompleto;
            usuarioEditar.Rut = model.Rut;
            usuarioEditar.Email = model.Email;
            usuarioEditar.Telefono = model.Telefono;
            usuarioEditar.Rol = model.Rol;
            usuarioEditar.Cargo = model.Cargo;
            usuarioEditar.Departamento = model.Departamento;
            usuarioEditar.Activo = model.Activo;

            // Actualizar contraseña si se proporcionó una nueva
            if (!string.IsNullOrEmpty(model.Password))
            {
                usuarioEditar.PasswordHash = HashPassword(model.Password);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usuario {model.Nombre} actualizado exitosamente";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            if (!await EsCamila())
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // No permitir eliminar a Camila a sí misma
            if (usuario.Email == "camila.flores@primar.cl")
            {
                TempData["ErrorMessage"] = "No puedes eliminar tu propio usuario";
                return RedirectToAction("Index");
            }

            // Verificar si tiene rendiciones asociadas
            var tieneRendiciones = await _context.Rendiciones.AnyAsync(r => r.UsuarioId == id);
            if (tieneRendiciones)
            {
                TempData["ErrorMessage"] = $"No se puede eliminar el usuario {usuario.Nombre} porque tiene rendiciones asociadas";
                return RedirectToAction("Index");
            }

            // Eliminar notificaciones asociadas
            var notificaciones = _context.Notificaciones.Where(n => n.UsuarioId == id);
            _context.Notificaciones.RemoveRange(notificaciones);

            // Eliminar usuario físicamente
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usuario {usuario.Nombre} eliminado exitosamente";
            return RedirectToAction("Index");
        }

        private async Task ActualizarNotificacionesNoLeidas(int userId)
        {
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido)
                .CountAsync();
            
            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
        }
    }
} 
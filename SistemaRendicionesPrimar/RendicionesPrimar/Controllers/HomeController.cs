using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using RendicionesPrimar.Models.ViewModels;
using System.Security.Claims;

namespace RendicionesPrimar.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
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

            // Si no se encuentra en usuarios, buscar en aprobadores usando el modelo plano
            var aprobador = await _context.Database.SqlQueryRaw<AprobadorSimple>("SELECT * FROM aprobadores WHERE id = {0}", userId).FirstOrDefaultAsync();
            return aprobador?.Nombre ?? "Usuario";
        }

        // Método para obtener los datos completos del usuario buscando en ambas tablas
        private async Task<object?> ObtenerUsuarioCompleto(int userId)
        {
            // Primero buscar en la tabla usuarios
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario != null)
            {
                return usuario;
            }

            // Si no se encuentra en usuarios, buscar en aprobadores usando el modelo plano
            var aprobador = await _context.Database.SqlQueryRaw<AprobadorSimple>("SELECT * FROM aprobadores WHERE id = {0}", userId).FirstOrDefaultAsync();
            return aprobador;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas específicas por rol
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido && n.TipoRol == GetTipoRol(userRole))
                .CountAsync();

            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
            ViewBag.UserRole = userRole;
            ViewBag.UserName = nombreUsuario;

            // Redirigir a las vistas de /Views/Home/ según el rol
            switch (userRole)
            {
                case "empleado":
                    return await DashboardEmpleado();
                case "aprobador1":
                    return await DashboardSupervisor();
                case "aprobador2":
                    return await DashboardGerente();
                case "admin":
                    return await DashboardAdmin();
                default:
                    return await DashboardEmpleado();
            }
        }

        private string GetTipoRol(string userRole)
        {
            return userRole switch
            {
                "empleado" => "empleado",
                "aprobador1" => "supervisor",
                "aprobador2" => "gerente",
                "admin" => "admin",
                _ => "empleado"
            };
        }

        public async Task<IActionResult> DashboardEmpleado()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Verificar que sea empleado
            if (userRole != "empleado")
            {
                return Forbid();
            }

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido)
                .CountAsync();

            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
            ViewBag.UserRole = userRole;
            ViewBag.UserName = nombreUsuario;

            // Crear el modelo de dashboard para empleado
            var modelo = new DashboardViewModel
            {
                UserName = nombreUsuario,
                UserRole = userRole,
                NotificacionesNoLeidas = notificacionesNoLeidas,
                TotalRendiciones = await _context.Rendiciones
                    .Where(r => r.UsuarioId == userId)
                    .CountAsync(),
                RendicionesPendientes = await _context.Rendiciones
                    .Where(r => r.UsuarioId == userId && r.Estado == "pendiente")
                    .CountAsync(),
                RendicionesAprobadas = await _context.Rendiciones
                    .Where(r => r.UsuarioId == userId && (r.Estado == "aprobado_2" || r.Estado == "pagado"))
                    .CountAsync(),
                MontoTotal = await _context.Rendiciones
                    .Where(r => r.UsuarioId == userId)
                    .SumAsync(r => r.MontoTotal)
            };

            // Obtener últimas rendiciones del empleado
            var ultimasRendiciones = await _context.Rendiciones
                .Where(r => r.UsuarioId == userId)
                .OrderByDescending(r => r.FechaCreacion)
                .Take(5)
                .ToListAsync();

            modelo.UltimasRendiciones = ultimasRendiciones.Select(r => 
                $"Rendición {r.NumeroTicket} - {r.Titulo} ({r.Estado})").ToList();

            return View("DashboardEmpleado", modelo);
        }

        public async Task<IActionResult> DashboardSupervisor()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Verificar que sea supervisor
            if (userRole != "aprobador1")
            {
                return Forbid();
            }

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido)
                .CountAsync();

            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
            ViewBag.UserRole = userRole;
            ViewBag.UserName = nombreUsuario;

            // Crear el modelo de dashboard para supervisor
            var modelo = new DashboardViewModel
            {
                UserName = nombreUsuario,
                UserRole = userRole,
                NotificacionesNoLeidas = notificacionesNoLeidas,
                TotalRendiciones = await _context.Rendiciones
                    .Where(r => r.Estado == "pendiente" || r.Estado == "aprobado_2")
                    .CountAsync(),
                RendicionesPendientes = await _context.Rendiciones
                    .Where(r => r.Estado == "pendiente")
                    .CountAsync(),
                RendicionesAprobadas = await _context.Rendiciones
                    .Where(r => r.Estado == "aprobado_2")
                    .CountAsync(),
                MontoTotal = 0 // Los supervisores no ven montos totales
            };

            // Obtener últimas rendiciones pendientes de aprobación
            var ultimasRendiciones = await _context.Rendiciones
                .Include(r => r.Usuario)
                .Where(r => r.Estado == "pendiente" || r.Estado == "aprobado_2")
                .OrderByDescending(r => r.FechaCreacion)
                .Take(5)
                .ToListAsync();

            modelo.UltimasRendiciones = ultimasRendiciones.Select(r => 
                $"Rendición {r.NumeroTicket} de {r.Usuario?.Nombre} - {r.Estado}").ToList();

            return View("DashboardSupervisor", modelo);
        }

        public async Task<IActionResult> DashboardGerente()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Verificar que sea gerente
            if (userRole != "aprobador2")
            {
                return Forbid();
            }

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido)
                .CountAsync();

            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
            ViewBag.UserRole = userRole;
            ViewBag.UserName = nombreUsuario;

            // Crear el modelo de dashboard para gerente
            var modelo = new DashboardViewModel
            {
                UserName = nombreUsuario,
                UserRole = userRole,
                NotificacionesNoLeidas = notificacionesNoLeidas,
                TotalRendiciones = await _context.Rendiciones
                    .Where(r => r.Estado == "aprobado_1")
                    .CountAsync(),
                RendicionesPendientes = await _context.Rendiciones
                    .Where(r => r.Estado == "aprobado_1")
                    .CountAsync(),
                RendicionesAprobadas = 0,
                MontoTotal = await _context.Rendiciones
                    .Where(r => r.Estado == "aprobado_1")
                    .SumAsync(r => r.MontoTotal)
            };

            // Obtener últimas rendiciones pendientes de aprobación final
            var ultimasRendiciones = await _context.Rendiciones
                .Include(r => r.Usuario)
                .Where(r => r.Estado == "aprobado_1")
                .OrderByDescending(r => r.FechaCreacion)
                .Take(5)
                .ToListAsync();

            modelo.UltimasRendiciones = ultimasRendiciones.Select(r => 
                $"Rendición {r.NumeroTicket} de {r.Usuario?.Nombre} - Pendiente aprobación final").ToList();

            return View("DashboardGerente", modelo);
        }

        public async Task<IActionResult> DashboardAdmin()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Verificar que sea admin
            if (userRole != "admin")
            {
                return Forbid();
            }

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido)
                .CountAsync();

            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
            ViewBag.UserRole = userRole;
            ViewBag.UserName = nombreUsuario;

            // Crear el modelo de dashboard para admin
            var modelo = new DashboardViewModel
            {
                UserName = nombreUsuario,
                UserRole = userRole,
                NotificacionesNoLeidas = notificacionesNoLeidas,
                TotalRendiciones = await _context.Rendiciones.CountAsync(),
                RendicionesPendientes = await _context.Rendiciones
                    .Where(r => r.Estado == "pendiente" || r.Estado == "aprobado_1")
                    .CountAsync(),
                RendicionesAprobadas = await _context.Rendiciones
                    .Where(r => r.Estado == "aprobado_2" || r.Estado == "pagado")
                    .CountAsync(),
                MontoTotal = await _context.Rendiciones.SumAsync(r => r.MontoTotal)
            };

            // Obtener últimas rendiciones generales
            var ultimasRendiciones = await _context.Rendiciones
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.FechaCreacion)
                .Take(5)
                .ToListAsync();

            modelo.UltimasRendiciones = ultimasRendiciones.Select(r => 
                $"Rendición {r.NumeroTicket} de {r.Usuario?.Nombre} - {r.Estado}").ToList();

            return View("DashboardAdmin", modelo);
        }

        public async Task<IActionResult> Notificaciones()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Redirigir a notificaciones específicas por rol
            switch (userRole)
            {
                case "empleado":
                    return RedirectToAction("Notificaciones", "Empleados");
                case "aprobador1":
                    return RedirectToAction("Notificaciones", "Supervisores");
                case "aprobador2":
                    return RedirectToAction("Notificaciones", "Gerentes");
                case "admin":
                    // Para admin, mantener en HomeController
                    var notificaciones = await _context.Notificaciones
                        .Include(n => n.Rendicion)
                        .Where(n => n.UsuarioId == userId && n.TipoRol == "admin")
                        .OrderByDescending(n => n.FechaCreacion)
                        .ToListAsync();

                    ViewBag.NotificacionesNoLeidas = notificaciones.Count(n => !n.Leido);
                    ViewBag.UserName = nombreUsuario;

                    return View(notificaciones);
                default:
                    return RedirectToAction("Notificaciones", "Empleados");
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarcarNotificacionLeidaYVer(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notificacion = await _context.Notificaciones.Include(n => n.Rendicion)
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);
            if (notificacion != null && !notificacion.Leido)
            {
                notificacion.Leido = true;
                await _context.SaveChangesAsync();
            }
            if (notificacion?.RendicionId != null)
            {
                return RedirectToAction("Detalle", "Rendiciones", new { id = notificacion.RendicionId });
            }
            return RedirectToAction("Notificaciones");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarNotificacion(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var notificacion = await _context.Notificaciones.FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId);
            if (notificacion != null)
            {
                _context.Notificaciones.Remove(notificacion);
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        public async Task<IActionResult> Ayuda()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            // Obtener el usuario para el nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            
            ViewBag.UserName = nombreUsuario;
            
            return View();
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
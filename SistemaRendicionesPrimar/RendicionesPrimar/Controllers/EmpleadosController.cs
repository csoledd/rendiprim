using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using RendicionesPrimar.Models.ViewModels;
using System.Security.Claims;

namespace RendicionesPrimar.Controllers
{
    [Authorize(Roles = "empleado")]
    public class EmpleadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmpleadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "empleado";

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas específicas de empleados
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido && n.TipoRol == "empleado")
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

            return View("Dashboard", modelo);
        }

        public async Task<IActionResult> MisRendiciones()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;

            var rendiciones = await _context.Rendiciones
                .Where(r => r.UsuarioId == userId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View("MisRendiciones", rendiciones);
        }

        public async Task<IActionResult> Notificaciones()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Obtener solo notificaciones específicas de empleados
            var notificaciones = await _context.Notificaciones
                .Include(n => n.Rendicion)
                .Where(n => n.UsuarioId == userId && n.TipoRol == "empleado")
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            ViewBag.NotificacionesNoLeidas = notificaciones.Count(n => !n.Leido);
            ViewBag.UserName = nombreUsuario;

            return View("Notificaciones", notificaciones);
        }

        [HttpGet]
        [Route("Empleados/Perfil")]
        public async Task<IActionResult> Perfil()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null)
                return NotFound();
            var viewModel = new InformacionPersonalViewModel
            {
                NombreCompleto = usuario.NombreCompleto,
                Rut = usuario.Rut,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Cargo = usuario.Cargo,
                Departamento = usuario.Departamento
            };
            return View(viewModel);
        }

        [HttpPost]
        [Route("Empleados/Perfil")]
        public async Task<IActionResult> Perfil(InformacionPersonalViewModel model)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!ModelState.IsValid)
                return View(model);
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null)
                return NotFound();
            usuario.NombreCompleto = model.NombreCompleto;
            usuario.Rut = model.Rut;
            usuario.Email = model.Email;
            usuario.Telefono = model.Telefono;
            usuario.Cargo = model.Cargo;
            usuario.Departamento = model.Departamento;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Información personal actualizada exitosamente";
            return RedirectToAction("Perfil");
        }

        [HttpPost]
        public async Task<IActionResult> MarcarNotificacionLeida(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId && n.TipoRol == "empleado");

            if (notificacion != null)
            {
                notificacion.Leido = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        private async Task<string> ObtenerNombreUsuario(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            return usuario?.Nombre ?? "Usuario";
        }
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using RendicionesPrimar.Models.ViewModels;
using System.Security.Claims;

namespace RendicionesPrimar.Controllers
{
    [Authorize(Roles = "aprobador2")]
    public class GerentesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GerentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "aprobador2";

            // Obtener el usuario completo con su nombre
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Contar notificaciones no leídas específicas de gerentes
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido && n.TipoRol == "gerente")
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

            return View("Dashboard", modelo);
        }

        public async Task<IActionResult> RendicionesPendientes()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;

            var rendiciones = await _context.Rendiciones
                .Include(r => r.Usuario)
                .Where(r => r.Estado == "aprobado_1")
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View("RendicionesPendientes", rendiciones);
        }

        public async Task<IActionResult> Notificaciones()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);

            // Obtener solo notificaciones específicas de gerentes
            var notificaciones = await _context.Notificaciones
                .Include(n => n.Rendicion)
                .Where(n => n.UsuarioId == userId && n.TipoRol == "gerente")
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            ViewBag.NotificacionesNoLeidas = notificaciones.Count(n => !n.Leido);
            ViewBag.UserName = nombreUsuario;

            return View("Notificaciones", notificaciones);
        }

        public async Task<IActionResult> Reportes()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;

            // Estadísticas para reportes de gerente
            var estadisticas = new
            {
                TotalPendientesAprobacion = await _context.Rendiciones.Where(r => r.Estado == "aprobado_1").CountAsync(),
                TotalAprobadas = await _context.Rendiciones.Where(r => r.Estado == "aprobado_2").CountAsync(),
                TotalPagadas = await _context.Rendiciones.Where(r => r.Estado == "pagado").CountAsync(),
                TotalRechazadas = await _context.Rendiciones.Where(r => r.Estado == "rechazado").CountAsync(),
                MontoTotalPendiente = await _context.Rendiciones.Where(r => r.Estado == "aprobado_1").SumAsync(r => r.MontoTotal),
                MontoTotalAprobado = await _context.Rendiciones.Where(r => r.Estado == "aprobado_2").SumAsync(r => r.MontoTotal),
                RendicionesPorMes = await _context.Rendiciones
                    .Where(r => r.FechaCreacion >= DateTime.Now.AddMonths(-1))
                    .CountAsync()
            };

            return View("Reportes", estadisticas);
        }

        public async Task<IActionResult> AnalisisFinanciero()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var nombreUsuario = await ObtenerNombreUsuario(userId);
            ViewBag.UserName = nombreUsuario;

            // Análisis financiero detallado
            var analisis = new
            {
                MontoTotalPendiente = await _context.Rendiciones.Where(r => r.Estado == "aprobado_1").SumAsync(r => r.MontoTotal),
                MontoTotalAprobado = await _context.Rendiciones.Where(r => r.Estado == "aprobado_2").SumAsync(r => r.MontoTotal),
                MontoTotalPagado = await _context.Rendiciones.Where(r => r.Estado == "pagado").SumAsync(r => r.MontoTotal),
                PromedioRendicion = await _context.Rendiciones.AverageAsync(r => r.MontoTotal),
                RendicionesPorDepartamento = await _context.Rendiciones
                    .GroupBy(r => r.Departamento)
                    .Select(g => new { Departamento = g.Key, Total = g.Sum(r => r.MontoTotal) })
                    .ToListAsync()
            };

            return View("AnalisisFinanciero", analisis);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarNotificacionLeida(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId && n.TipoRol == "gerente");

            if (notificacion != null)
            {
                notificacion.Leido = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        [HttpGet]
        [Route("Gerentes/Perfil")]
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
        [Route("Gerentes/Perfil")]
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

        private async Task<string> ObtenerNombreUsuario(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            return usuario?.Nombre ?? "Usuario";
        }
    }
} 
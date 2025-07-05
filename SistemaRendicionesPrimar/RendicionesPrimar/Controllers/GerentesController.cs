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
    [Authorize(Roles = "aprobador2")]
    public class GerentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RendicionService _rendicionService;

        public GerentesController(ApplicationDbContext context, RendicionService rendicionService)
        {
            _context = context;
            _rendicionService = rendicionService;
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

            modelo.UltimasRendiciones = ultimasRendiciones;

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
            var notificaciones = await _context.Notificaciones
                .Include(n => n.Rendicion)
                .Where(n => n.UsuarioId == userId && n.TipoRol == "gerente")
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();
            return View(notificaciones);
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

        [HttpPost]
        public async Task<IActionResult> AprobarRendicion(int id, string? comentarios)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ok = await _rendicionService.AprobarSegundaInstanciaAsync(id, userId, comentarios);
            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo aprobar la rendición";
                return RedirectToAction("RendicionesPendientes");
            }
            TempData["SuccessMessage"] = "Rendición aprobada";
            return RedirectToAction("RendicionesPendientes");
        }

        [HttpPost]
        public async Task<IActionResult> RechazarRendicion(int id, string? comentarios)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ok = await _rendicionService.RechazarAsync(id, comentarios ?? "Rechazada por el gerente", userId);
            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo rechazar la rendición";
                return RedirectToAction("RendicionesPendientes");
            }
            TempData["SuccessMessage"] = "Rendición rechazada";
            return RedirectToAction("RendicionesPendientes");
        }

        [HttpGet]
        [Route("/perfil-gerente")]
        public async Task<IActionResult> PerfilFijo()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var aprobador = await _context.Aprobadores.FindAsync(userId);
            var nombreUsuario = aprobador != null ? aprobador.Nombre : "Usuario";
            ViewBag.UserName = nombreUsuario;
            if (aprobador == null)
                return Content("Aprobador no encontrado");
            var viewModel = new InformacionPersonalViewModel
            {
                Nombre = aprobador.Nombre,
                Apellidos = aprobador.Apellidos,
                Rut = aprobador.Rut,
                Email = aprobador.Email,
                Cargo = aprobador.Cargo,
                Departamento = aprobador.Departamento,
                Telefono = aprobador.Telefono
            };
            return View("Perfil", viewModel);
        }

        [Authorize(Roles = "aprobador2")]
        [HttpPost]
        [Route("/perfil-gerente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PerfilFijo(InformacionPersonalViewModel model)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!ModelState.IsValid)
                return Content("Modelo inválido");

            var aprobador = await _context.Aprobadores.FindAsync(userId);
            if (aprobador == null)
                return Content("Aprobador no encontrado");

            // Solo actualiza los campos permitidos
            aprobador.Nombre = model.Nombre;
            aprobador.Apellidos = model.Apellidos;
            aprobador.Rut = model.Rut;
            aprobador.Email = model.Email;
            aprobador.Telefono = model.Telefono;
            aprobador.Cargo = model.Cargo;
            aprobador.Departamento = model.Departamento;
            // NO toques: password_hash, rol, activo, mfa, etc.

            _context.Entry(aprobador).Property(x => x.Nombre).IsModified = true;
            _context.Entry(aprobador).Property(x => x.Apellidos).IsModified = true;
            _context.Entry(aprobador).Property(x => x.Rut).IsModified = true;
            _context.Entry(aprobador).Property(x => x.Email).IsModified = true;
            _context.Entry(aprobador).Property(x => x.Telefono).IsModified = true;
            _context.Entry(aprobador).Property(x => x.Cargo).IsModified = true;
            _context.Entry(aprobador).Property(x => x.Departamento).IsModified = true;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Cambios guardados!";
            return RedirectToAction("PerfilFijo");
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var aprobador = await _context.Aprobadores.FindAsync(userId);
            var nombreUsuario = aprobador != null ? aprobador.Nombre : "Usuario";
            ViewBag.UserName = nombreUsuario;
            if (aprobador == null)
                return Content("Aprobador no encontrado");
            var viewModel = new InformacionPersonalViewModel
            {
                Nombre = aprobador.Nombre,
                Apellidos = aprobador.Apellidos,
                Rut = aprobador.Rut,
                Email = aprobador.Email,
                Cargo = aprobador.Cargo,
                Departamento = aprobador.Departamento,
                Telefono = aprobador.Telefono
            };
            return View("Perfil", viewModel);
        }

        private async Task<string> ObtenerNombreUsuario(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            return usuario?.Nombre ?? "Usuario";
        }
    }
}
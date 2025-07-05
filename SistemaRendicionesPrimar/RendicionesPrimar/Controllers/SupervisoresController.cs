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
    [Authorize(Roles = "aprobador1")]
    public class SupervisoresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RendicionService _rendicionService;

        public SupervisoresController(ApplicationDbContext context, RendicionService rendicionService)
        {
            _context = context;
            _rendicionService = rendicionService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "aprobador1";

            // Obtener el usuario completo con su nombre usando SQL directo
            var aprobador = await _context.Aprobadores
                .FromSqlRaw("SELECT * FROM aprobadores WHERE id = {0}", userId)
                .FirstOrDefaultAsync();
            var nombreUsuario = aprobador != null && !string.IsNullOrWhiteSpace(aprobador.Nombre) ? aprobador.Nombre : "Usuario";
            ViewBag.UserName = nombreUsuario;

            // Contar notificaciones no leídas específicas de supervisores
            var notificacionesNoLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == userId && !n.Leido && n.TipoRol == "supervisor")
                .CountAsync();

            ViewBag.NotificacionesNoLeidas = notificacionesNoLeidas;
            ViewBag.UserRole = userRole;

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

            modelo.UltimasRendiciones = ultimasRendiciones;

            return View("Dashboard", modelo);
        }

        public async Task<IActionResult> RendicionesPendientes()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "aprobador1";
            var aprobador = await _context.Aprobadores
                .FromSqlRaw("SELECT * FROM aprobadores WHERE id = {0}", userId)
                .FirstOrDefaultAsync();
            var nombreUsuario = aprobador != null && !string.IsNullOrWhiteSpace(aprobador.Nombre) ? aprobador.Nombre : "Usuario";
            ViewBag.UserName = nombreUsuario;

            var rendiciones = await _context.Rendiciones
                .Include(r => r.Usuario)
                .Where(r => r.Estado == "pendiente" || r.Estado == "aprobado_2")
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();

            return View("RendicionesPendientes", rendiciones);
        }

        public async Task<IActionResult> Notificaciones()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "aprobador1";
            var aprobador = await _context.Aprobadores
                .FromSqlRaw("SELECT * FROM aprobadores WHERE id = {0}", userId)
                .FirstOrDefaultAsync();
            var nombreUsuario = aprobador != null && !string.IsNullOrWhiteSpace(aprobador.Nombre) ? aprobador.Nombre : "Usuario";
            ViewBag.UserName = nombreUsuario;

            // Obtener solo notificaciones específicas de supervisores
            var notificaciones = await _context.Notificaciones
                .Include(n => n.Rendicion)
                .Where(n => n.UsuarioId == userId && n.TipoRol == "supervisor")
                .OrderByDescending(n => n.FechaCreacion)
                .ToListAsync();

            ViewBag.NotificacionesNoLeidas = notificaciones.Count(n => !n.Leido);

            return View("Notificaciones", notificaciones);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            var estadisticas = new
            {
                aprobadas = await _context.Rendiciones.Where(r => r.Estado == "aprobado_2").CountAsync(),
                pendientes = await _context.Rendiciones.Where(r => r.Estado == "pendiente").CountAsync()
            };
            
            return Json(estadisticas);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarNotificacionLeida(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var notificacion = await _context.Notificaciones
                .FirstOrDefaultAsync(n => n.Id == id && n.UsuarioId == userId && n.TipoRol == "supervisor");

            if (notificacion != null)
            {
                notificacion.Leido = true;
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // MÉTODOS PERFIL SIN ATRIBUTOS DE RUTA
        [AllowAnonymous]
        [HttpGet]
        [Route("/perfil-supervisor")]
        public async Task<IActionResult> PerfilFijo()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var aprobador = await _context.Aprobadores.FindAsync(userId);
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

        [Authorize(Roles = "aprobador1")]
        [HttpPost]
        [Route("/perfil-supervisor")]
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

        private async Task<string> ObtenerNombreUsuario(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            return usuario?.Nombre ?? "Usuario";
        }

        public IActionResult Ayuda()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AprobarRendicion(int id, string? comentarios)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var ok = await _rendicionService.AprobarPrimeraInstanciaAsync(id, userId, comentarios);
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
            var ok = await _rendicionService.RechazarAsync(id, comentarios ?? "Rechazada por el supervisor", userId);
            if (!ok)
            {
                TempData["ErrorMessage"] = "No se pudo rechazar la rendición";
                return RedirectToAction("RendicionesPendientes");
            }
            TempData["SuccessMessage"] = "Rendición rechazada";
            return RedirectToAction("RendicionesPendientes");
        }
    }
}
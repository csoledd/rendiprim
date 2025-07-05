using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;
using Microsoft.AspNetCore.SignalR;
using RendicionesPrimar.Services;

namespace RendicionesPrimar.Services
{
    public class RendicionService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IHubContext<NotificacionesHub> _hubContext;

        public RendicionService(ApplicationDbContext context, EmailService emailService, IHubContext<NotificacionesHub> hubContext)
        {
            _context = context;
            _emailService = emailService;
            _hubContext = hubContext;
        }

        public async Task<Rendicion> CrearRendicionAsync(int usuarioId, string titulo, string? descripcion, decimal montoTotal)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            var numeroTicket = await GenerarNumeroTicketUnicoAsync();
            
            var rendicion = new Rendicion
            {
                NumeroTicket = numeroTicket,
                UsuarioId = usuarioId,
                Titulo = titulo,
                Descripcion = descripcion,
                MontoTotal = montoTotal,
                Estado = "pendiente",
                FechaCreacion = DateTime.Now,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Rut = usuario.Rut,
                Telefono = usuario.Telefono,
                Cargo = usuario.Cargo,
                Departamento = usuario.Departamento
            };

            _context.Rendiciones.Add(rendicion);
            await _context.SaveChangesAsync();

            // Notificar y establecer estado inicial usando la función centralizada
            await CambiarEstadoRendicionAsync(rendicion.Id, "pendiente", usuarioId.ToString());

            // Notificar actividad reciente en tiempo real
            await _hubContext.Clients.All.SendAsync("ActualizarActividadReciente", new {
                Id = rendicion.Id,
                NumeroTicket = rendicion.NumeroTicket,
                Titulo = rendicion.Titulo,
                MontoTotal = rendicion.MontoTotal,
                Estado = rendicion.Estado,
                Usuario = usuario.Nombre ?? "",
                FechaCreacion = rendicion.FechaCreacion
            });

            return rendicion;
        }

        public async Task<bool> AprobarPrimeraInstanciaAsync(int rendicionId, int aprobadorId, string? comentarios = null)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId && r.Estado == "pendiente");

            if (rendicion == null)
                return false;

            rendicion.Aprobador1Id = aprobadorId;
            rendicion.FechaAprobacion1 = DateTime.Now;
            if (!string.IsNullOrEmpty(comentarios))
            {
                rendicion.ComentariosAprobador = comentarios;
            }
            await _context.SaveChangesAsync();

            await CambiarEstadoRendicionAsync(rendicionId, "aprobado_1", aprobadorId.ToString());
            return true;
        }

        public async Task<bool> AprobarSegundaInstanciaAsync(int rendicionId, int aprobadorId, string? comentarios = null)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId && r.Estado == "aprobado_1");

            if (rendicion == null)
                return false;

            rendicion.Aprobador2Id = aprobadorId;
            rendicion.FechaAprobacion2 = DateTime.Now;
            if (!string.IsNullOrEmpty(comentarios))
            {
                var comentarioExistente = rendicion.ComentariosAprobador ?? "";
                rendicion.ComentariosAprobador = comentarioExistente + $"\nAprobación final: {comentarios}";
            }
            await _context.SaveChangesAsync();

            await CambiarEstadoRendicionAsync(rendicionId, "aprobado_2", aprobadorId.ToString());
            return true;
        }

        public async Task<bool> MarcarComoPagadaAsync(int rendicionId)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId && r.Estado == "aprobado_2");

            if (rendicion == null)
                return false;

            rendicion.FechaPago = DateTime.Now;
            await _context.SaveChangesAsync();

            await CambiarEstadoRendicionAsync(rendicionId, "pagado", "0");
            return true;
        }

        public async Task<bool> RechazarAsync(int rendicionId, string motivo, int? aprobadorId = null)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId);

            if (rendicion == null)
                return false;

            rendicion.ComentariosAprobador = motivo;
            if (aprobadorId.HasValue)
            {
                if (rendicion.Estado == "pendiente") rendicion.Aprobador1Id = aprobadorId;
                else if (rendicion.Estado == "aprobado_1") rendicion.Aprobador2Id = aprobadorId;
            }
            await _context.SaveChangesAsync();

            await CambiarEstadoRendicionAsync(rendicionId, "rechazado", aprobadorId?.ToString() ?? "0");
            return true;
        }

        public async Task<List<Rendicion>> ObtenerRendicionesPorUsuarioAsync(int usuarioId)
        {
            return await _context.Rendiciones
                .Include(r => r.ArchivosAdjuntos)
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<Rendicion>> ObtenerRendicionesPendientesAprobacion1Async()
        {
            return await _context.Rendiciones
                .Include(r => r.Usuario)
                .Include(r => r.ArchivosAdjuntos)
                .Where(r => r.Estado == "pendiente")
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<Rendicion>> ObtenerRendicionesPendientesAprobacion2Async()
        {
            return await _context.Rendiciones
                .Include(r => r.Usuario)
                .Include(r => r.ArchivosAdjuntos)
                .Where(r => r.Estado == "aprobado_1")
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        public async Task<List<Rendicion>> ObtenerRendicionesPendientesPagoAsync()
        {
            return await _context.Rendiciones
                .Include(r => r.Usuario)
                .Include(r => r.ArchivosAdjuntos)
                .Where(r => r.Estado == "aprobado_2")
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        public async Task CambiarEstadoRendicionAsync(int rendicionId, string nuevoEstado, string usuarioAccionId)
        {
            var rendicion = await _context.Rendiciones.Include(r => r.Usuario).FirstOrDefaultAsync(r => r.Id == rendicionId);
            if (rendicion == null) return;

            string mensajeEmpleado = null;

            switch (nuevoEstado)
            {
                case "pendiente":
                    await NotificarAprobador1Async(rendicion);
                    mensajeEmpleado = $"Tu rendición {rendicion.NumeroTicket} fue enviada y está pendiente de aprobación del supervisor.";
                    break;
                case "aprobado_1":
                    await NotificarAprobador2Async(rendicion);
                    mensajeEmpleado = $"Tu rendición {rendicion.NumeroTicket} fue aprobada por el supervisor y está pendiente de aprobación del gerente.";
                    break;
                case "aprobado_2":
                    mensajeEmpleado = $"Tu rendición {rendicion.NumeroTicket} fue aprobada por el gerente y está lista para ser pagada.";
                    // Notificar al supervisor para pago
                    await NotificarParaPagoAsync(rendicion);
                    break;
                case "pagado":
                    mensajeEmpleado = $"Tu rendición {rendicion.NumeroTicket} fue pagada.";
                    break;
                case "rechazado":
                    mensajeEmpleado = $"Tu rendición {rendicion.NumeroTicket} fue rechazada.";
                    break;
            }

            if (!string.IsNullOrEmpty(mensajeEmpleado))
            {
                await NotificarEmpleadoAsync(rendicion, mensajeEmpleado);
            }

            rendicion.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
        }

        private async Task<string> GenerarNumeroTicketUnicoAsync()
        {
            string numeroTicket;
            bool existe;

            do
            {
                var random = new Random();
                numeroTicket = $"RND-{random.Next(100000, 999999)}";
                existe = await _context.Rendiciones.AnyAsync(r => r.NumeroTicket == numeroTicket);
            }
            while (existe);

            return numeroTicket;
        }

        private async Task NotificarAprobador1Async(Rendicion rendicion)
        {
            // Notificar solo a los supervisores (aprobador1) que deben aprobar esta rendición
            var supervisores = await _context.Usuarios
                .Where(u => u.Rol == "aprobador1" && u.Activo)
                .ToListAsync();

            foreach (var aprobador1 in supervisores)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador1.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Nueva rendición {rendicion.NumeroTicket} de {rendicion.Usuario?.Nombre} requiere tu aprobación.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "supervisor"
                };
                _context.Notificaciones.Add(notificacion);
                await _emailService.EnviarNotificacionAsync(
                    aprobador1.Email,
                    "Nueva Rendición Pendiente",
                    $"La rendición {rendicion.NumeroTicket} requiere tu aprobación."
                );
            }
            await _context.SaveChangesAsync();
        }

        private async Task NotificarAprobador2Async(Rendicion rendicion)
        {
            // Notificar solo a los gerentes (aprobador2) que deben aprobar esta rendición
            var gerentes = await _context.Usuarios
                .Where(u => u.Rol == "aprobador2" && u.Activo)
                .ToListAsync();

            foreach (var aprobador2 in gerentes)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador2.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Rendición {rendicion.NumeroTicket} requiere tu aprobación final.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "gerente"
                };
                _context.Notificaciones.Add(notificacion);
                await _hubContext.Clients.All.SendAsync("NuevaNotificacion", notificacion.Mensaje);
            }
            await _context.SaveChangesAsync();
        }

        private async Task NotificarParaPagoAsync(Rendicion rendicion)
        {
            // Notificar a todos los supervisores para proceder con el pago
            var supervisores = await _context.Usuarios
                .Where(u => u.Rol == "aprobador1" && u.Activo)
                .ToListAsync();

            foreach (var supervisor in supervisores)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = supervisor.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Rendición {rendicion.NumeroTicket} aprobada por el gerente. Proceder con el pago.",
                    Leido = false,
                    FechaCreacion = DateTime.Now,
                    TipoRol = "supervisor"
                };

                _context.Notificaciones.Add(notificacion);

                // Enviar email (opcional)
                await _emailService.EnviarNotificacionAsync(
                    supervisor.Email,
                    "Rendición Lista para Pago",
                    $"La rendición {rendicion.NumeroTicket} está lista para ser pagada."
                );
            }
            
            await _context.SaveChangesAsync();
        }

        private async Task NotificarEmpleadoAsync(Rendicion rendicion, string mensaje)
        {
            // Notificar solo al empleado dueño de la rendición
            if (rendicion.Usuario != null)
            {
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
                await _emailService.EnviarNotificacionAsync(
                    rendicion.Usuario.Email,
                    "Actualización de tu rendición",
                    mensaje
                );
            }
        }
    }
}
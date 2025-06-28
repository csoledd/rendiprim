using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Data;
using RendicionesPrimar.Models;

namespace RendicionesPrimar.Services
{
    public class RendicionService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public RendicionService(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<Rendicion> CrearRendicionAsync(int usuarioId, string titulo, string? descripcion, decimal montoTotal)
        {
            var numeroTicket = await GenerarNumeroTicketUnicoAsync();
            
            var rendicion = new Rendicion
            {
                NumeroTicket = numeroTicket,
                UsuarioId = usuarioId,
                Titulo = titulo,
                Descripcion = descripcion,
                MontoTotal = montoTotal,
                Estado = "pendiente",
                FechaCreacion = DateTime.Now
            };

            _context.Rendiciones.Add(rendicion);
            await _context.SaveChangesAsync();

            // Notificar al primer aprobador
            await NotificarAprobador1Async(rendicion);

            return rendicion;
        }

        public async Task<bool> AprobarPrimeraInstanciaAsync(int rendicionId, int aprobadorId, string? comentarios = null)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId && r.Estado == "pendiente");

            if (rendicion == null)
                return false;

            rendicion.Estado = "aprobado_1";
            rendicion.Aprobador1Id = aprobadorId;
            rendicion.FechaAprobacion1 = DateTime.Now;
            
            if (!string.IsNullOrEmpty(comentarios))
            {
                rendicion.ComentariosAprobador = comentarios;
            }

            await _context.SaveChangesAsync();

            // Notificar al segundo aprobador
            await NotificarAprobador2Async(rendicion);

            return true;
        }

        public async Task<bool> AprobarSegundaInstanciaAsync(int rendicionId, int aprobadorId, string? comentarios = null)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId && r.Estado == "aprobado_1");

            if (rendicion == null)
                return false;

            rendicion.Estado = "aprobado_2";
            rendicion.Aprobador2Id = aprobadorId;
            rendicion.FechaAprobacion2 = DateTime.Now;
            
            if (!string.IsNullOrEmpty(comentarios))
            {
                var comentarioExistente = rendicion.ComentariosAprobador ?? "";
                rendicion.ComentariosAprobador = comentarioExistente + $"\nAprobación final: {comentarios}";
            }

            await _context.SaveChangesAsync();

            // Notificar al primer aprobador para proceder con el pago
            await NotificarParaPagoAsync(rendicion);

            return true;
        }

        public async Task<bool> MarcarComoPagadaAsync(int rendicionId)
        {
            var rendicion = await _context.Rendiciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == rendicionId && r.Estado == "aprobado_2");

            if (rendicion == null)
                return false;

            rendicion.Estado = "pagado";
            rendicion.FechaPago = DateTime.Now;

            await _context.SaveChangesAsync();

            // Notificar al empleado que se ha pagado
            await NotificarPagadoAsync(rendicion);

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
            var aprobador1 = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Rol == "aprobador1");

            if (aprobador1 != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador1.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Nueva rendición {rendicion.NumeroTicket} de {rendicion.Usuario?.Nombre} requiere aprobación.",
                    Leido = false,
                    FechaCreacion = DateTime.Now
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                // Enviar email (opcional)
                await _emailService.EnviarNotificacionAsync(
                    aprobador1.Email,
                    "Nueva Rendición Pendiente",
                    $"La rendición {rendicion.NumeroTicket} requiere su aprobación."
                );
            }
        }

        private async Task NotificarAprobador2Async(Rendicion rendicion)
        {
            var aprobador2 = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Rol == "aprobador2");

            if (aprobador2 != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador2.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Rendición {rendicion.NumeroTicket} de {rendicion.Usuario?.Nombre} requiere tu aprobación final.",
                    Leido = false,
                    FechaCreacion = DateTime.Now
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                // Enviar email (opcional)
                await _emailService.EnviarNotificacionAsync(
                    aprobador2.Email,
                    "Rendición para Aprobación Final",
                    $"La rendición {rendicion.NumeroTicket} requiere su aprobación final."
                );
            }
        }

        private async Task NotificarParaPagoAsync(Rendicion rendicion)
        {
            var aprobador1 = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Rol == "aprobador1");

            if (aprobador1 != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = aprobador1.Id,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Rendición {rendicion.NumeroTicket} aprobada por Don Juan. Proceder con el pago.",
                    Leido = false,
                    FechaCreacion = DateTime.Now
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                // Enviar email (opcional)
                await _emailService.EnviarNotificacionAsync(
                    aprobador1.Email,
                    "Rendición Lista para Pago",
                    $"La rendición {rendicion.NumeroTicket} está lista para ser pagada."
                );
            }
        }

        private async Task NotificarPagadoAsync(Rendicion rendicion)
        {
            if (rendicion.Usuario != null)
            {
                var notificacion = new Notificacion
                {
                    UsuarioId = rendicion.UsuarioId,
                    RendicionId = rendicion.Id,
                    Mensaje = $"Su rendición {rendicion.NumeroTicket} ha sido pagada exitosamente.",
                    Leido = false,
                    FechaCreacion = DateTime.Now
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                // Enviar email (opcional)
                await _emailService.EnviarNotificacionAsync(
                    rendicion.Usuario.Email,
                    "Rendición Pagada",
                    $"Su rendición {rendicion.NumeroTicket} ha sido pagada exitosamente."
                );
            }
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RendicionesPrimar.Services
{
    public class NotificacionesHub : Hub
    {
        // Métodos para enviar notificaciones en tiempo real a los gerentes
        public async Task SendActividadReciente(object actividad)
        {
            await Clients.All.SendAsync("ActualizarActividadReciente", actividad);
        }
    }
} 
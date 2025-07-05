using System.ComponentModel.DataAnnotations;

namespace RendicionesPrimar.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        public int RendicionId { get; set; }
        
        public string? Mensaje { get; set; }
        
        public bool Leido { get; set; } = false;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        
        // Nuevo campo para separar notificaciones por rol
        public string? TipoRol { get; set; } // "empleado", "supervisor", "gerente", "admin"
        
        // Navegación
        // public virtual Usuario? Usuario { get; set; }
        public virtual Rendicion? Rendicion { get; set; }
    }
}
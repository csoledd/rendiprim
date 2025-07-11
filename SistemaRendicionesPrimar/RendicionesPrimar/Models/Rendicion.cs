using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RendicionesPrimar.Models
{
    public class Rendicion
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string? NumeroTicket { get; set; }

        public int UsuarioId { get; set; }

        [StringLength(200)]
        public string? Titulo { get; set; }

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        [StringLength(50)]
        public string? Estado { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Campos para aprobaciones
        public int? Aprobador1Id { get; set; }
        public DateTime? FechaAprobacion1 { get; set; }

        public int? Aprobador2Id { get; set; }
        public DateTime? FechaAprobacion2 { get; set; }

        public DateTime? FechaPago { get; set; }

        [StringLength(1000)]
        public string? ComentariosAprobador { get; set; }

        // Propiedades adicionales para compatibilidad con el contexto
        public int? AprobadoPor1 { get; set; }
        public int? AprobadoPor2 { get; set; }

        // Información personal del empleado al momento de la rendición
        [StringLength(100)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [StringLength(100)]
        [Column("apellidos")]
        public string? Apellidos { get; set; }

        [StringLength(20)]
        public string? Rut { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Cargo { get; set; }

        [StringLength(100)]
        public string? Departamento { get; set; }

        // Relaciones
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey("Aprobador1Id")]
        public virtual Usuario? Aprobador1 { get; set; }

        [ForeignKey("Aprobador2Id")]
        public virtual Usuario? Aprobador2 { get; set; }

        public virtual ICollection<ArchivoAdjunto> ArchivosAdjuntos { get; set; } = new List<ArchivoAdjunto>();
        public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

        public string EstadoLegible {
            get {
                if (string.IsNullOrWhiteSpace(Estado)) return "Desconocido";
                return Estado switch {
                    "pendiente" => "Pendiente",
                    "aprobado_1" => "Aceptada",
                    "aprobado_2" => "Aceptada",
                    "pagado" => "Aceptada",
                    "rechazado" => "Rechazada",
                    _ => "Desconocido"
                };
            }
        }
    }
}
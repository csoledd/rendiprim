using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RendicionesPrimar.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Rut { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Cargo { get; set; }

        [StringLength(100)]
        public string? Departamento { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Propiedades para recuperación de contraseña
        [StringLength(6)]
        public string? CodigoRecuperacion { get; set; }

        public DateTime? CodigoRecuperacionExpira { get; set; }

        // Propiedades para MFA (Multi-Factor Authentication)
        [StringLength(32)]
        [Column("mfa_secret_key")]
        public string? MfaSecretKey { get; set; }

        [Column("mfa_habilitado")]
        public bool MfaHabilitado { get; set; } = false;

        [Column("mfa_fecha_habilitacion")]
        public DateTime? MfaFechaHabilitacion { get; set; }

        // Código de verificación temporal para MFA
        [StringLength(6)]
        [Column("mfa_codigo_verificacion")]
        public string? MfaCodigoVerificacion { get; set; }

        [Column("mfa_codigo_expira")]
        public DateTime? MfaCodigoExpira { get; set; }

        // Relaciones
        public virtual ICollection<Rendicion> Rendiciones { get; set; } = new List<Rendicion>();
        public virtual ICollection<Rendicion> RendicionesAprobadas1 { get; set; } = new List<Rendicion>();
        public virtual ICollection<Rendicion> RendicionesAprobadas2 { get; set; } = new List<Rendicion>();
    }
}
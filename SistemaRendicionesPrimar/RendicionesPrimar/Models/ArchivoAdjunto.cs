using System.ComponentModel.DataAnnotations;

namespace RendicionesPrimar.Models
{
    public class ArchivoAdjunto
    {
        public int Id { get; set; }

        [Required]
        public int RendicionId { get; set; }

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TipoArchivo { get; set; } = string.Empty;

        public long TamanoArchivo { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;

        // Relación
        public virtual Rendicion Rendicion { get; set; } = null!;
    }
}
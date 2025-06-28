using System.ComponentModel.DataAnnotations;

namespace RendicionesPrimar.Models.ViewModels
{
    public class CrearRendicionViewModel
    {
        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "El monto debe ser positivo y menor a 9.999.999.999")]
        public decimal MontoTotal { get; set; }

        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder 100 caracteres")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT es requerido")]
        [StringLength(20, ErrorMessage = "El RUT no puede exceder 20 caracteres")]
        public string Rut { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres")]
        public string? Cargo { get; set; }

        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        public string? Departamento { get; set; }

        public List<IFormFile> Archivos { get; set; } = new List<IFormFile>();
    }
}
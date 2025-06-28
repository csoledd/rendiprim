using System.ComponentModel.DataAnnotations;

namespace RendicionesPrimar.Models.ViewModels
{
    public class InformacionPersonalViewModel
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [StringLength(100, ErrorMessage = "El nombre completo no puede exceder 100 caracteres")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUT es requerido")]
        [StringLength(20, ErrorMessage = "El RUT no puede exceder 20 caracteres")]
        [Display(Name = "RUT")]
        public string Rut { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres")]
        [Display(Name = "Cargo")]
        public string? Cargo { get; set; }

        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        [Display(Name = "Departamento")]
        public string? Departamento { get; set; }
    }
} 
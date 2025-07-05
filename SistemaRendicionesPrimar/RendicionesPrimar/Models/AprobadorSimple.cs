using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RendicionesPrimar.Models
{
    public class AprobadorSimple
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("nombre")]
        public string? Nombre { get; set; }
        [Column("apellidos")]
        public string? Apellidos { get; set; }
        [Column("rut")]
        public string? Rut { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("telefono")]
        public string? Telefono { get; set; }
        [Column("password_hash")]
        public string? PasswordHash { get; set; }
        [Column("rol")]
        public string? Rol { get; set; }
        [Column("cargo")]
        public string? Cargo { get; set; }
        [Column("departamento")]
        public string? Departamento { get; set; }
        [Column("activo")]
        public bool Activo { get; set; }
        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }
        // Agrega aquí solo los campos que realmente existen en la tabla aprobadores
    }
} 
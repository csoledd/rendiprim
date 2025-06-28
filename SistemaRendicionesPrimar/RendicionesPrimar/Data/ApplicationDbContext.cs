using Microsoft.EntityFrameworkCore;
using RendicionesPrimar.Models;

namespace RendicionesPrimar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rendicion> Rendiciones { get; set; }
        public DbSet<ArchivoAdjunto> ArchivosAdjuntos { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar nombres de tablas
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Rendicion>().ToTable("rendiciones");
            modelBuilder.Entity<ArchivoAdjunto>().ToTable("archivos_adjuntos");
            modelBuilder.Entity<Notificacion>().ToTable("notificaciones");
            
            // Configurar relaciones
            modelBuilder.Entity<Rendicion>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Rendiciones)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Rendicion>()
                .HasOne(r => r.Aprobador1)
                .WithMany(u => u.RendicionesAprobadas1)
                .HasForeignKey(r => r.Aprobador1Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<Rendicion>()
                .HasOne(r => r.Aprobador2)
                .WithMany(u => u.RendicionesAprobadas2)
                .HasForeignKey(r => r.Aprobador2Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configurar propiedades específicas para MySQL
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100);
                entity.Property(e => e.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(100);
                entity.Property(e => e.Rut).HasColumnName("rut").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
                entity.Property(e => e.Rol).HasColumnName("rol").HasMaxLength(20);
                entity.Property(e => e.Cargo).HasColumnName("cargo").HasMaxLength(100);
                entity.Property(e => e.Departamento).HasColumnName("departamento").HasMaxLength(100);
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            });
            
            modelBuilder.Entity<Rendicion>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.NumeroTicket).HasColumnName("numero_ticket").HasMaxLength(20);
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.Titulo).HasColumnName("titulo").HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
                entity.Property(e => e.MontoTotal).HasColumnName("monto_total").HasColumnType("decimal(10,2)");
                entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20);
                entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
                entity.Property(e => e.FechaAprobacion1).HasColumnName("fecha_aprobacion_1");
                entity.Property(e => e.FechaAprobacion2).HasColumnName("fecha_aprobacion_2");
                entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
                entity.Property(e => e.Aprobador1Id).HasColumnName("aprobador_1_id");
                entity.Property(e => e.Aprobador2Id).HasColumnName("aprobador_2_id");
                entity.Property(e => e.ComentariosAprobador).HasColumnName("comentarios_aprobador");
                entity.Property(e => e.AprobadoPor1).HasColumnName("aprobado_por_1");
                entity.Property(e => e.AprobadoPor2).HasColumnName("aprobado_por_2");
                entity.Property(e => e.NombreCompleto).HasColumnName("nombre_completo").HasMaxLength(100);
                entity.Property(e => e.Rut).HasColumnName("rut").HasMaxLength(20);
                entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20);
                entity.Property(e => e.Cargo).HasColumnName("cargo").HasMaxLength(100);
                entity.Property(e => e.Departamento).HasColumnName("departamento").HasMaxLength(100);
            });
            
            modelBuilder.Entity<ArchivoAdjunto>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.RendicionId).HasColumnName("rendicion_id");
                entity.Property(e => e.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255);
                entity.Property(e => e.RutaArchivo).HasColumnName("ruta_archivo").HasMaxLength(500);
                entity.Property(e => e.TipoArchivo).HasColumnName("tipo_archivo").HasMaxLength(50);
                entity.Property(e => e.TamanoArchivo).HasColumnName("tamano_archivo");
                entity.Property(e => e.FechaSubida).HasColumnName("fecha_subida");
            });
            
            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
                entity.Property(e => e.RendicionId).HasColumnName("rendicion_id");
                entity.Property(e => e.Mensaje).HasColumnName("mensaje");
                entity.Property(e => e.Leido).HasColumnName("leido");
                entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
                entity.Property(e => e.TipoRol).HasColumnName("tipo_rol").HasMaxLength(20);
            });
        }
    }
}
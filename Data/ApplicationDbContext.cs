using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using San_Agustin_Final.Models;

namespace San_Agustin_Final.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para cada modelo
        public DbSet<Bobeda> Bobedas { get; set; }
        public DbSet<Precio> Precios { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<HistorialPago> HistorialPagos { get; set; }
        public DbSet<TipoBobeda> TiposBobeda { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Contrato> Dashboard { get; set; }
        public DbSet<Mensajes> Mensajes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones (sin la relación entre Bobeda y TipoBobeda)
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Cliente) // Relación con Cliente
                .WithMany() // Un cliente puede tener muchas reservas
                .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Precio)
                .WithMany()
                .HasForeignKey(r => r.IdPrecio)
                .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada

            modelBuilder.Entity<HistorialPago>()
                .HasOne(hp => hp.Reserva)
                .WithMany()
                .HasForeignKey(hp => hp.IdReserva)
                .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada


            modelBuilder.Entity<Reserva>()
               .HasOne(hp => hp.Cliente)
               .WithMany()
               .HasForeignKey(hp => hp.IdCliente)
               .OnDelete(DeleteBehavior.Restrict);
           
            modelBuilder.Entity<Contrato>()
                .HasOne(c => c.Reserva)
                .WithMany()
                .HasForeignKey(c => c.IdReserva)
                .OnDelete(DeleteBehavior.Restrict); // Evita la eliminación en cascada

            // Especificar el tipo de columna y la precisión para las propiedades decimal
            modelBuilder.Entity<Precio>()
                .Property(p => p.PrecioValor)
                .HasColumnType("decimal(10, 2)"); // 10 dígitos en total, 2 después del punto decimal

            modelBuilder.Entity<HistorialPago>()
                .Property(hp => hp.Monto)
                .HasColumnType("decimal(10, 2)"); // 10 dígitos en total, 2 después del punto decimal

            // Configuración de la entidad TipoBobeda
            modelBuilder.Entity<TipoBobeda>(entity =>
            {
                // Configura el nombre de la tabla (si es diferente al nombre de la clase)
                entity.ToTable("TipoBobeda");

                // Configura las propiedades
                entity.Property(t => t.Nombre)
                    .IsRequired()  // Esto asegura que la propiedad sea obligatoria
                    .HasMaxLength(100); // Limita el tamaño máximo de la cadena a 100 caracteres

                entity.Property(t => t.Descripcion)
                    .HasMaxLength(500); // Limita el tamaño máximo de la descripción

                // Configura la clave primaria
                entity.HasKey(t => t.Id); // Asumiendo que 'Id' es la clave primaria

                // Si deseas añadir alguna otra restricción o propiedad
                // ejemplo de una propiedad que podría ser única
                entity.HasIndex(t => t.Nombre)
                    .IsUnique(); // Asegura que 'Nombre' sea único en la base de datos
            });
        }
    }
}

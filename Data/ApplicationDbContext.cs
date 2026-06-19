using Microsoft.EntityFrameworkCore;
using LOGIN.Models;

namespace LOGIN.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<CarritoItem> CarritoItems { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ciudad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Edad).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Cantidad).IsRequired();

                // 🔥 CAMBIO IMPORTANTE: Para PostgreSQL usamos HasColumnType en lugar de HasPrecision
                entity.Property(e => e.Precio)
                      .HasColumnType("decimal(18,2)"); // ← Nuevo formato para PostgreSQL

                // 🔥 CAMBIO IMPORTANTE: GETDATE() no existe en PostgreSQL, usamos NOW()
                entity.Property(e => e.FechaRegistro)
                      .HasDefaultValueSql("NOW()"); // ← Cambiado de GETDATE() a NOW()
            });

            modelBuilder.Entity<CarritoItem>(entity =>
            {
                entity.ToTable("CarritoItems");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Pedidos");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.UsuarioId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.Estado)
                      .HasConversion<int>();
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("PedidoDetalles");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Producto)
                      .WithMany()
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
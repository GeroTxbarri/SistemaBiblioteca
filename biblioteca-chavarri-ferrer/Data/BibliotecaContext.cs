using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Models;

namespace biblioteca_chavarri_ferrer.Data;

public class BibliotecaContext : DbContext
{
    // ── DbSets ──────────────────────────────────────────────────────────────
    public DbSet<TipoSocio> TiposSocio { get; set; }
    public DbSet<EstadoPrestamo> EstadosPrestamo { get; set; }
    public DbSet<EstadoReserva> EstadosReserva { get; set; }
    public DbSet<Libro> Libros { get; set; }
    public DbSet<Socio> Socios { get; set; }
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    // ── Configuración de la conexión ─────────────────────────────────────────
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite("Data Source=biblioteca.db");
    }

    // ── Configuración del modelo ─────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Libro ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Libro>(entity =>
        {
            entity.HasKey(l => l.ISBN);
            entity.Property(l => l.ISBN).HasMaxLength(20);
            entity.Property(l => l.Titulo).IsRequired();
            entity.Property(l => l.Autor).IsRequired();
            entity.Property(l => l.Genero).IsRequired();
        });

        // ── TipoSocio ────────────────────────────────────────────────────────
        modelBuilder.Entity<TipoSocio>(entity =>
        {
            entity.HasKey(ts => ts.Id);
            entity.Property(ts => ts.Nombre).IsRequired().HasMaxLength(50);
            entity.Property(ts => ts.MultaPorDia).HasColumnType("REAL");
        });

        // ── EstadoPrestamo ───────────────────────────────────────────────────
        modelBuilder.Entity<EstadoPrestamo>(entity =>
        {
            entity.HasKey(ep => ep.Id);
            entity.Property(ep => ep.Nombre).IsRequired().HasMaxLength(20);
        });

        // ── EstadoReserva ────────────────────────────────────────────────────
        modelBuilder.Entity<EstadoReserva>(entity =>
        {
            entity.HasKey(er => er.Id);
            entity.Property(er => er.Nombre).IsRequired().HasMaxLength(20);
        });

        // ── Socio ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Socio>(entity =>
        {
            entity.HasKey(s => s.NroSocio);
            entity.Property(s => s.Nombre).IsRequired();
            entity.Property(s => s.Apellido).IsRequired();
            entity.Property(s => s.Email).IsRequired();

            entity.HasOne(s => s.TipoSocio)
                  .WithMany(ts => ts.Socios)
                  .HasForeignKey(s => s.TipoSocioId);
        });

        // ── Prestamo ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Prestamo>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Multa).HasColumnType("REAL");

            entity.HasOne(p => p.Socio)
                  .WithMany(s => s.Prestamos)
                  .HasForeignKey(p => p.SocioId);

            entity.HasOne(p => p.Libro)
                  .WithMany(l => l.Prestamos)
                  .HasForeignKey(p => p.LibroISBN);

            entity.HasOne(p => p.EstadoPrestamo)
                  .WithMany(ep => ep.Prestamos)
                  .HasForeignKey(p => p.EstadoPrestamoId);
        });

        // ── Reserva ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.HasOne(r => r.Socio)
                  .WithMany(s => s.Reservas)
                  .HasForeignKey(r => r.SocioId);

            entity.HasOne(r => r.Libro)
                  .WithMany(l => l.Reservas)
                  .HasForeignKey(r => r.LibroISBN);

            entity.HasOne(r => r.EstadoReserva)
                  .WithMany(er => er.Reservas)
                  .HasForeignKey(r => r.EstadoReservaId);
        });
    }
}

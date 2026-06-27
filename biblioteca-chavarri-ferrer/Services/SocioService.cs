using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;
using biblioteca_chavarri_ferrer.Models;

namespace biblioteca_chavarri_ferrer.Services;

public class SocioService
{
    private readonly BibliotecaContext _context;

    public SocioService(BibliotecaContext context)
    {
        _context = context;
    }

    public void VerDetalleSocio()
    {
        Console.WriteLine("=== DETALLE DE SOCIO ===\n");

        Console.Write("Ingresá el número de socio: ");
        if (!int.TryParse(Console.ReadLine(), out int nroSocio))
        {
            Console.WriteLine("Número de socio inválido.");
            return;
        }

        var socio = _context.Socios
            .Include(s => s.TipoSocio)
            .FirstOrDefault(s => s.NroSocio == nroSocio);

        if (socio == null)
        {
            Console.WriteLine("Socio no encontrado.");
            return;
        }

        // ── Datos del socio ──────────────────────────────────────────────────
        string estadoSocio = socio.Activo ? "Activo" : "INACTIVO";
        Console.WriteLine($"\n--- Socio Nro {socio.NroSocio} ---");
        Console.WriteLine($"  Nombre:     {socio.Nombre} {socio.Apellido}");
        Console.WriteLine($"  Email:      {socio.Email}");
        Console.WriteLine($"  Tipo:       {socio.TipoSocio.Nombre} (max {socio.TipoSocio.MaxLibros} libros, {socio.TipoSocio.DiasPrestamo} días)");
        Console.WriteLine($"  Estado:     {estadoSocio}");

        // ── Préstamos activos ────────────────────────────────────────────────
        var prestamosActivos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.EstadoPrestamo)
            .Where(p => p.SocioId == nroSocio &&
                        (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"))
            .OrderBy(p => p.FechaVencimiento)
            .ToList();

        Console.WriteLine($"\n--- Préstamos activos ({prestamosActivos.Count}) ---");
        if (!prestamosActivos.Any())
        {
            Console.WriteLine("  Sin préstamos activos.");
        }
        else
        {
            foreach (var p in prestamosActivos)
            {
                bool vencido = p.FechaVencimiento < DateTime.Today;
                string alerta = vencido ? " ⚠ VENCIDO" : "";
                Console.WriteLine($"  • {p.Libro.Titulo} — Vence: {p.FechaVencimiento:dd/MM/yyyy}{alerta}");
            }
        }

        // ── Historial de devoluciones ────────────────────────────────────────
        var devueltos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.EstadoPrestamo)
            .Where(p => p.SocioId == nroSocio && p.EstadoPrestamo.Nombre == "Devuelto")
            .OrderByDescending(p => p.FechaDevolucion)
            .ToList();

        Console.WriteLine($"\n--- Historial de devoluciones ({devueltos.Count}) ---");
        if (!devueltos.Any())
        {
            Console.WriteLine("  Sin devoluciones registradas.");
        }
        else
        {
            foreach (var p in devueltos)
            {
                string multaStr = p.Multa.HasValue ? $" | Multa: ${p.Multa:F2}" : "";
                Console.WriteLine($"  • {p.Libro.Titulo} — Devuelto: {p.FechaDevolucion:dd/MM/yyyy}{multaStr}");
            }
        }

        // ── Multas pendientes ────────────────────────────────────────────────
        // Una multa está "pendiente" si el préstamo tiene multa y aún no fue devuelto
        // (préstamos vencidos con multa implícita que se calculará al devolver)
        // También mostramos multas ya registradas en préstamos devueltos
        var multasRegistradas = devueltos
            .Where(p => p.Multa.HasValue && p.Multa > 0)
            .Sum(p => p.Multa ?? 0);

        Console.WriteLine($"\n--- Multas ---");
        Console.WriteLine($"  Total multas acumuladas: ${multasRegistradas:F2}");

        // Préstamos vencidos (multa potencial aún no cobrada)
        var vencidos = prestamosActivos.Where(p => p.FechaVencimiento < DateTime.Today).ToList();
        if (vencidos.Any())
        {
            Console.WriteLine($"  Préstamos vencidos con multa por cobrar al devolver:");
            foreach (var p in vencidos)
            {
                int diasDemora = (DateTime.Today - p.FechaVencimiento).Days;
                decimal multaPotencial = diasDemora * socio.TipoSocio.MultaPorDia;
                Console.WriteLine($"    • {p.Libro.Titulo} — {diasDemora} días de demora — Multa estimada: ${multaPotencial:F2}");
            }
        }

        // ── Reservas activas ─────────────────────────────────────────────────
        var reservasPendientes = _context.Reservas
            .Include(r => r.Libro)
            .Where(r => r.SocioId == nroSocio && r.EstadoReserva.Nombre == "Pendiente")
            .OrderBy(r => r.FechaReserva)
            .ToList();

        Console.WriteLine($"\n--- Reservas pendientes ({reservasPendientes.Count}) ---");
        if (!reservasPendientes.Any())
        {
            Console.WriteLine("  Sin reservas pendientes.");
        }
        else
        {
            foreach (var r in reservasPendientes)
            {
                Console.WriteLine($"  • {r.Libro.Titulo} — Reservado el: {r.FechaReserva:dd/MM/yyyy}");
            }
        }

        Console.WriteLine();
    }
}

using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;

namespace biblioteca_chavarri_ferrer.Services;

public class ReporteService
{
    private readonly BibliotecaContext _context;

    public ReporteService(BibliotecaContext context)
    {
        _context = context;
    }

    // ── Reporte 1: Top 5 libros más prestados ────────────────────────────────
    public void LibrosMasPrestados()
    {
        Console.WriteLine("=== TOP 5 LIBROS MÁS PRESTADOS ===\n");

        var top5 = _context.Libros
            .Select(l => new
            {
                l.Titulo,
                l.Autor,
                l.Genero,
                TotalPrestamos = l.Prestamos.Count()
            })
            .OrderByDescending(l => l.TotalPrestamos)
            .Take(5)
            .ToList();

        if (!top5.Any())
        {
            Console.WriteLine("No hay préstamos registrados.");
            return;
        }

        int posicion = 1;
        foreach (var libro in top5)
        {
            Console.WriteLine($"  {posicion}. {libro.Titulo} — {libro.Autor} [{libro.Genero}]");
            Console.WriteLine($"     Total de préstamos: {libro.TotalPrestamos}");
            posicion++;
        }

        Console.WriteLine();
    }

    // ── Reporte 2: Socios con multas pendientes ──────────────────────────────
    public void SociosConMultasPendientes()
    {
        Console.WriteLine("=== SOCIOS CON MULTAS PENDIENTES ===\n");

        // Multa pendiente = préstamo devuelto con Multa > 0
        var socios = _context.Socios
            .Include(s => s.TipoSocio)
            .Where(s => s.Prestamos.Any(p => p.Multa != null && p.Multa > 0))
            .Select(s => new
            {
                s.NroSocio,
                s.Nombre,
                s.Apellido,
                TipoSocio = s.TipoSocio.Nombre,
                TotalMulta = s.Prestamos
                    .Where(p => p.Multa != null && p.Multa > 0)
                    .Sum(p => p.Multa ?? 0)
            })
            .OrderByDescending(s => s.TotalMulta)
            .ToList();

        if (!socios.Any())
        {
            Console.WriteLine("No hay socios con multas registradas.");
            return;
        }

        foreach (var s in socios)
        {
            Console.WriteLine($"  Socio Nro {s.NroSocio} — {s.Nombre} {s.Apellido} [{s.TipoSocio}]");
            Console.WriteLine($"  Total multas: ${s.TotalMulta:F2}");
            Console.WriteLine();
        }
    }
}

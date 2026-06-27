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
}

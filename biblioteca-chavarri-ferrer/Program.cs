using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;
using biblioteca_chavarri_ferrer.Models;
using biblioteca_chavarri_ferrer.Services;

// ── Inicialización de la base de datos ──────────────────────────────────────
using var context = new BibliotecaContext();

// Crea la DB y ejecuta el script SQL si no existe
bool dbExiste = File.Exists("biblioteca.db");
context.Database.EnsureCreated();

if (!dbExiste)
{
    Console.WriteLine("Base de datos no encontrada. Inicializando con datos de ejemplo...");
    string scriptPath = "biblioteca.sql";

    if (File.Exists(scriptPath))
    {
        string sql = File.ReadAllText(scriptPath);

        // Ejecutar bloque a bloque (SQLite no admite múltiples statements en una sola llamada)
        var statements = sql
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrWhiteSpace(s) && !s.StartsWith("--") && !s.StartsWith("PRAGMA"));

        // Habilitar foreign keys primero
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");

        foreach (var statement in statements)
        {
            try { context.Database.ExecuteSqlRaw(statement); }
            catch { /* Ignorar errores de tablas ya existentes */ }
        }

        Console.WriteLine("Datos iniciales cargados correctamente.\n");
    }
    else
    {
        Console.WriteLine("ADVERTENCIA: No se encontró biblioteca.sql. La base de datos está vacía.\n");
    }
}

// ── Mostrar todos los libros disponibles al iniciar ─────────────────────────
MostrarLibrosDisponibles(context);

// ── Menú principal ───────────────────────────────────────────────────────────
bool salir = false;
while (!salir)
{
    Console.WriteLine("\n╔══════════════════════════════════════════╗");
    Console.WriteLine("║   SISTEMA DE GESTIÓN DE BIBLIOTECA       ║");
    Console.WriteLine("╠══════════════════════════════════════════╣");
    Console.WriteLine("║  1. Realizar un préstamo                 ║");
    Console.WriteLine("║  2. Registrar devolución                 ║");
    Console.WriteLine("║  3. Realizar una reserva                 ║");
    Console.WriteLine("║  4. Ver detalle de socio                 ║");
    Console.WriteLine("╠══════════════════════════════════════════╣");
    Console.WriteLine("║  ── REPORTES ──                          ║");
    Console.WriteLine("║  5. Libros más prestados (Top 5)         ║");
    Console.WriteLine("║  6. Socios con multas pendientes         ║");
    Console.WriteLine("║  7. Préstamos vencidos                   ║");
    Console.WriteLine("║  8. Disponibilidad de un libro           ║");
    Console.WriteLine("║  9. Historial de un socio                ║");
    Console.WriteLine("╠══════════════════════════════════════════╣");
    Console.WriteLine("║  0. Salir                                ║");
    Console.WriteLine("╚══════════════════════════════════════════╝");
    Console.Write("\nSeleccioná una opción: ");

    string? opcion = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (opcion)
    {
        case "1":
            new PrestamoService(context).RealizarPrestamo();
            break;
        case "0":
            salir = true;
            Console.WriteLine("¡Hasta luego!");
            break;
        default:
            Console.WriteLine("Opción no implementada aún. Próximamente...");
            break;
    }
}

// ── Funciones locales ────────────────────────────────────────────────────────

static void MostrarLibrosDisponibles(BibliotecaContext ctx)
{
    // Copias disponibles = CantidadCopias - préstamos activos (EstadoPrestamoId = 1 o 3)
    var libros = ctx.Libros
        .Select(l => new
        {
            l.ISBN,
            l.Titulo,
            l.Autor,
            l.Genero,
            l.CantidadCopias,
            CopiasPrestadas = l.Prestamos.Count(p =>
                p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido")
        })
        .OrderBy(l => l.Titulo)
        .ToList();

    Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                        LIBROS DEL CATÁLOGO                                  ║");
    Console.WriteLine("╠══════════╦══════════════════════════════════╦══════════════════╦═══════════╣");
    Console.WriteLine("║ ISBN     ║ Título                           ║ Autor            ║ Disponib. ║");
    Console.WriteLine("╠══════════╬══════════════════════════════════╬══════════════════╬═══════════╣");

    foreach (var l in libros)
    {
        int disponibles = l.CantidadCopias - l.CopiasPrestadas;
        string disponiblesStr = disponibles > 0
            ? $"{disponibles}/{l.CantidadCopias}"
            : "Sin copias";

        Console.WriteLine($"║ {l.ISBN[..10],-8} ║ {l.Titulo,-32} ║ {l.Autor,-16} ║ {disponiblesStr,-9} ║");
    }

    Console.WriteLine("╚══════════╩══════════════════════════════════╩══════════════════╩═══════════╝");
    Console.WriteLine($"\nTotal: {libros.Count} libro(s) en catálogo.");
}

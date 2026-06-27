using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;
using biblioteca_chavarri_ferrer.Models;

namespace biblioteca_chavarri_ferrer.Services;

public class ReservaService
{
    private readonly BibliotecaContext _context;

    public ReservaService(BibliotecaContext context)
    {
        _context = context;
    }

    public void RealizarReserva()
    {
        Console.WriteLine("=== REALIZAR RESERVA ===\n");

        // ── 1. Buscar socio ──────────────────────────────────────────────────
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

        Console.WriteLine($"Socio: {socio.Nombre} {socio.Apellido} — Tipo: {socio.TipoSocio.Nombre}");

        // ── RN-01: Socio activo ──────────────────────────────────────────────
        if (!socio.Activo)
        {
            Console.WriteLine("ERROR: El socio está inactivo y no puede realizar reservas.");
            return;
        }

        // ── 2. Buscar libro ──────────────────────────────────────────────────
        Console.Write("\nIngresá título o autor del libro a reservar: ");
        string busqueda = Console.ReadLine()?.Trim() ?? "";

        var libros = _context.Libros
            .Where(l => l.Titulo.ToLower().Contains(busqueda.ToLower()) ||
                        l.Autor.ToLower().Contains(busqueda.ToLower()))
            .ToList();

        if (!libros.Any())
        {
            Console.WriteLine("No se encontraron libros con ese criterio.");
            return;
        }

        Console.WriteLine("\nResultados:");
        for (int i = 0; i < libros.Count; i++)
        {
            var lib = libros[i];
            int copiasTomadas = _context.Prestamos
                .Count(p => p.LibroISBN == lib.ISBN &&
                            (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"));
            int disponibles = lib.CantidadCopias - copiasTomadas;

            int reservasPendientes = _context.Reservas
                .Count(r => r.LibroISBN == lib.ISBN && r.EstadoReserva.Nombre == "Pendiente");

            string dispStr = disponibles > 0 ? $"{disponibles} disponible(s)" : "Sin copias";
            Console.WriteLine($"  {i + 1}. {lib.Titulo} — {lib.Autor} | {dispStr} | {reservasPendientes} reserva(s) pendiente(s)");
        }

        Console.Write("\nElegí el número del libro (0 para cancelar): ");
        if (!int.TryParse(Console.ReadLine(), out int seleccion) || seleccion == 0 || seleccion > libros.Count)
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        var libroElegido = libros[seleccion - 1];

        // ── Verificar si hay copias disponibles (reserva para libros sin stock) ─
        int copiasTomadas2 = _context.Prestamos
            .Count(p => p.LibroISBN == libroElegido.ISBN &&
                        (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"));
        int copiasDisponibles = libroElegido.CantidadCopias - copiasTomadas2;

        if (copiasDisponibles > 0)
        {
            Console.WriteLine($"\nEl libro \"{libroElegido.Titulo}\" tiene {copiasDisponibles} copia(s) disponible(s).");
            Console.Write("¿Preferís hacer un préstamo directo en lugar de una reserva? (s/n): ");
            string resp = Console.ReadLine()?.Trim().ToLower() ?? "s";
            if (resp == "s")
            {
                Console.WriteLine("Usá la opción 1 del menú para realizar el préstamo.");
                return;
            }
        }

        // ── RN-08: Un socio no puede tener más de una reserva activa para el mismo libro ─
        bool yaReservado = _context.Reservas
            .Any(r => r.SocioId == nroSocio &&
                      r.LibroISBN == libroElegido.ISBN &&
                      r.EstadoReserva.Nombre == "Pendiente");

        if (yaReservado)
        {
            Console.WriteLine("Ya tenés una reserva pendiente para este libro.");
            return;
        }

        // ── 3. Confirmar y registrar reserva ─────────────────────────────────
        Console.WriteLine($"\nLibro: {libroElegido.Titulo} — {libroElegido.Autor}");
        Console.Write("¿Confirmás la reserva? (s/n): ");
        string confirmar = Console.ReadLine()?.Trim().ToLower() ?? "n";

        if (confirmar != "s")
        {
            Console.WriteLine("Reserva cancelada.");
            return;
        }

        var estadoPendiente = _context.EstadosReserva.First(e => e.Nombre == "Pendiente");

        var nuevaReserva = new Reserva
        {
            SocioId         = socio.NroSocio,
            LibroISBN       = libroElegido.ISBN,
            FechaReserva    = DateTime.Today,
            EstadoReservaId = estadoPendiente.Id
        };

        _context.Reservas.Add(nuevaReserva);
        _context.SaveChanges();

        Console.WriteLine($"\n✓ Reserva registrada exitosamente.");
        Console.WriteLine($"  Libro: {libroElegido.Titulo}");
        Console.WriteLine($"  Socio: {socio.Nombre} {socio.Apellido}");
        Console.WriteLine($"  Fecha: {DateTime.Today:dd/MM/yyyy}");
        Console.WriteLine("  Te avisaremos cuando el libro esté disponible.");
    }
}

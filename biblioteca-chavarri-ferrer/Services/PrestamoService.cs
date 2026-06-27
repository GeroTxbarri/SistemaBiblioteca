using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;
using biblioteca_chavarri_ferrer.Models;

namespace biblioteca_chavarri_ferrer.Services;

public class PrestamoService
{
    private readonly BibliotecaContext _context;

    public PrestamoService(BibliotecaContext context)
    {
        _context = context;
    }

    public void RealizarPrestamo()
    {
        Console.WriteLine("=== REALIZAR PRÉSTAMO ===\n");

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
            Console.WriteLine("ERROR: El socio está inactivo y no puede realizar préstamos.");
            return;
        }

        // ── RN-02: Sin multas pendientes ─────────────────────────────────────
        bool tieneMulta = _context.Prestamos
            .Any(p => p.SocioId == nroSocio && p.Multa != null && p.FechaDevolucion == null);

        if (tieneMulta)
        {
            Console.WriteLine("ERROR: El socio tiene multas pendientes. Debe abonarlas antes de realizar un nuevo préstamo.");
            return;
        }

        // ── RN-04: Verificar límite de libros simultáneos ────────────────────
        int prestamosActivos = _context.Prestamos
            .Count(p => p.SocioId == nroSocio &&
                        (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"));

        if (prestamosActivos >= socio.TipoSocio.MaxLibros)
        {
            Console.WriteLine($"ERROR: El socio ya tiene {prestamosActivos} préstamos activos (límite: {socio.TipoSocio.MaxLibros} para tipo {socio.TipoSocio.Nombre}).");
            return;
        }

        // ── 2. Buscar libro ──────────────────────────────────────────────────
        Console.Write("\nIngresá título o autor del libro a buscar: ");
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
            int copiasPrestadas = _context.Prestamos
                .Count(p => p.LibroISBN == lib.ISBN &&
                            (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"));
            int disponibles = lib.CantidadCopias - copiasPrestadas;

            Console.WriteLine($"  {i + 1}. {lib.Titulo} — {lib.Autor} [{lib.Genero}] | Disponibles: {disponibles}/{lib.CantidadCopias}");
        }

        Console.Write("\nElegí el número del libro (0 para cancelar): ");
        if (!int.TryParse(Console.ReadLine(), out int seleccion) || seleccion == 0 || seleccion > libros.Count)
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        var libroElegido = libros[seleccion - 1];

        // ── RN-03: Verificar disponibilidad ──────────────────────────────────
        int copiasTomadas = _context.Prestamos
            .Count(p => p.LibroISBN == libroElegido.ISBN &&
                        (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"));

        int copiasDisponibles = libroElegido.CantidadCopias - copiasTomadas;

        if (copiasDisponibles <= 0)
        {
            Console.WriteLine($"\nNo hay copias disponibles de \"{libroElegido.Titulo}\".");
            Console.Write("¿Querés hacer una reserva? (s/n): ");
            string resp = Console.ReadLine()?.Trim().ToLower() ?? "n";

            if (resp == "s")
            {
                OfrecerReserva(socio, libroElegido);
            }
            return;
        }

        // ── 3. Confirmar y registrar préstamo ────────────────────────────────
        Console.WriteLine($"\nLibro: {libroElegido.Titulo}");
        Console.WriteLine($"Fecha de vencimiento: {DateTime.Today.AddDays(socio.TipoSocio.DiasPrestamo):dd/MM/yyyy} ({socio.TipoSocio.DiasPrestamo} días)");
        Console.Write("¿Confirmás el préstamo? (s/n): ");
        string confirmar = Console.ReadLine()?.Trim().ToLower() ?? "n";

        if (confirmar != "s")
        {
            Console.WriteLine("Préstamo cancelado.");
            return;
        }

        // ── RN-05: Calcular FechaVencimiento ─────────────────────────────────
        var estadoActivo = _context.EstadosPrestamo.First(e => e.Nombre == "Activo");

        var nuevoPrestamo = new Prestamo
        {
            SocioId          = socio.NroSocio,
            LibroISBN        = libroElegido.ISBN,
            FechaPrestamo    = DateTime.Today,
            FechaVencimiento = DateTime.Today.AddDays(socio.TipoSocio.DiasPrestamo),
            FechaDevolucion  = null,
            EstadoPrestamoId = estadoActivo.Id,
            Multa            = null
        };

        _context.Prestamos.Add(nuevoPrestamo);
        _context.SaveChanges();

        Console.WriteLine($"\n✓ Préstamo registrado exitosamente.");
        Console.WriteLine($"  Libro:      {libroElegido.Titulo}");
        Console.WriteLine($"  Socio:      {socio.Nombre} {socio.Apellido}");
        Console.WriteLine($"  Vencimiento: {nuevoPrestamo.FechaVencimiento:dd/MM/yyyy}");
    }

    // ── Oferta de reserva cuando no hay copias ───────────────────────────────
    private void OfrecerReserva(Socio socio, Libro libro)
    {
        // RN-08: no puede tener reserva pendiente para el mismo libro
        bool yaReservado = _context.Reservas
            .Any(r => r.SocioId == socio.NroSocio &&
                      r.LibroISBN == libro.ISBN &&
                      r.EstadoReserva.Nombre == "Pendiente");

        if (yaReservado)
        {
            Console.WriteLine("Ya tenés una reserva pendiente para este libro.");
            return;
        }

        var estadoPendiente = _context.EstadosReserva.First(e => e.Nombre == "Pendiente");

        var reserva = new Reserva
        {
            SocioId         = socio.NroSocio,
            LibroISBN       = libro.ISBN,
            FechaReserva    = DateTime.Today,
            EstadoReservaId = estadoPendiente.Id
        };

        _context.Reservas.Add(reserva);
        _context.SaveChanges();

        Console.WriteLine($"✓ Reserva registrada para \"{libro.Titulo}\". Te avisaremos cuando esté disponible.");
    }
}

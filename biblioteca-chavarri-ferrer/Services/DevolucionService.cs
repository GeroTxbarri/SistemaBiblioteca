using Microsoft.EntityFrameworkCore;
using biblioteca_chavarri_ferrer.Data;
using biblioteca_chavarri_ferrer.Models;

namespace biblioteca_chavarri_ferrer.Services;

public class DevolucionService
{
    private readonly BibliotecaContext _context;

    public DevolucionService(BibliotecaContext context)
    {
        _context = context;
    }

    public void RegistrarDevolucion()
    {
        Console.WriteLine("=== REGISTRAR DEVOLUCIÓN ===\n");

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

        Console.WriteLine($"Socio: {socio.Nombre} {socio.Apellido}");

        // ── 2. Listar préstamos activos del socio ────────────────────────────
        var prestamosActivos = _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.EstadoPrestamo)
            .Where(p => p.SocioId == nroSocio &&
                        (p.EstadoPrestamo.Nombre == "Activo" || p.EstadoPrestamo.Nombre == "Vencido"))
            .ToList();

        if (!prestamosActivos.Any())
        {
            Console.WriteLine("Este socio no tiene préstamos activos para devolver.");
            return;
        }

        Console.WriteLine("\nPréstamos activos:");
        for (int i = 0; i < prestamosActivos.Count; i++)
        {
            var p = prestamosActivos[i];
            bool vencido = p.FechaVencimiento < DateTime.Today;
            string estado = vencido ? "⚠ VENCIDO" : "Al día";
            Console.WriteLine($"  {i + 1}. [{estado}] {p.Libro.Titulo} — Vence: {p.FechaVencimiento:dd/MM/yyyy}");
        }

        // ── 3. Seleccionar préstamo ──────────────────────────────────────────
        Console.Write("\nElegí el número del préstamo a devolver (0 para cancelar): ");
        if (!int.TryParse(Console.ReadLine(), out int seleccion) || seleccion == 0 || seleccion > prestamosActivos.Count)
        {
            Console.WriteLine("Operación cancelada.");
            return;
        }

        var prestamo = prestamosActivos[seleccion - 1];

        // ── 4. Registrar devolución ──────────────────────────────────────────
        DateTime hoy = DateTime.Today;
        prestamo.FechaDevolucion = hoy;

        // ── RN-06: Calcular multa si hay demora ──────────────────────────────
        var estadoDevuelto = _context.EstadosPrestamo.First(e => e.Nombre == "Devuelto");
        prestamo.EstadoPrestamoId = estadoDevuelto.Id;

        if (hoy > prestamo.FechaVencimiento)
        {
            int diasDemora = (hoy - prestamo.FechaVencimiento).Days;
            decimal multa = diasDemora * socio.TipoSocio.MultaPorDia;
            prestamo.Multa = multa;

            Console.WriteLine($"\n⚠ Devolución con demora de {diasDemora} día(s).");
            Console.WriteLine($"  Multa generada: ${multa:F2} ({diasDemora} días × ${socio.TipoSocio.MultaPorDia}/día)");
        }
        else
        {
            Console.WriteLine("\nDevolución en término. Sin multa.");
        }

        _context.SaveChanges();

        Console.WriteLine($"✓ Devolución registrada: \"{prestamo.Libro.Titulo}\" el {hoy:dd/MM/yyyy}.");

        // ── RN-07: Notificar reserva pendiente más antigua ───────────────────
        var reservaPendiente = _context.Reservas
            .Include(r => r.Socio)
            .Where(r => r.LibroISBN == prestamo.LibroISBN && r.EstadoReserva.Nombre == "Pendiente")
            .OrderBy(r => r.FechaReserva)
            .FirstOrDefault();

        if (reservaPendiente != null)
        {
            var estadoCumplida = _context.EstadosReserva.First(e => e.Nombre == "Cumplida");
            reservaPendiente.EstadoReservaId = estadoCumplida.Id;
            _context.SaveChanges();

            Console.WriteLine($"\n📢 AVISO: El libro \"{prestamo.Libro.Titulo}\" está disponible para:");
            Console.WriteLine($"   Socio Nro {reservaPendiente.SocioId} — {reservaPendiente.Socio.Nombre} {reservaPendiente.Socio.Apellido}");
            Console.WriteLine($"   (Reserva del {reservaPendiente.FechaReserva:dd/MM/yyyy} marcada como Cumplida)");
        }
    }
}

namespace biblioteca_chavarri_ferrer.Models;

public class Prestamo
{
    public int Id { get; set; }
    public int SocioId { get; set; }
    public string LibroISBN { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }  // null si aún no fue devuelto
    public int EstadoPrestamoId { get; set; }
    public decimal? Multa { get; set; }             // null si no hay multa pendiente

    // Navegación
    public Socio Socio { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
    public EstadoPrestamo EstadoPrestamo { get; set; } = null!;
}

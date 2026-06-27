namespace biblioteca_chavarri_ferrer.Models;

public class Reserva
{
    public int Id { get; set; }
    public int SocioId { get; set; }
    public string LibroISBN { get; set; } = string.Empty;
    public DateTime FechaReserva { get; set; }
    public int EstadoReservaId { get; set; }

    // Navegación
    public Socio Socio { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
    public EstadoReserva EstadoReserva { get; set; } = null!;
}

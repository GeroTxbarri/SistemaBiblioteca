namespace biblioteca_chavarri_ferrer.Models;

public class EstadoReserva
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty; // "Pendiente", "Cumplida", "Cancelada"

    // Navegación
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

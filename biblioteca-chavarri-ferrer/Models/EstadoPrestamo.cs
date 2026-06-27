namespace biblioteca_chavarri_ferrer.Models;

public class EstadoPrestamo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty; // "Activo", "Devuelto", "Vencido"

    // Navegación
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}

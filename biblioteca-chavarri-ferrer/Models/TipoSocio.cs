namespace biblioteca_chavarri_ferrer.Models;

public class TipoSocio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int MaxLibros { get; set; }
    public int DiasPrestamo { get; set; }
    public decimal MultaPorDia { get; set; }

    // Navegación
    public ICollection<Socio> Socios { get; set; } = new List<Socio>();
}

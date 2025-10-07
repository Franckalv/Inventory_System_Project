using System.ComponentModel.DataAnnotations;

namespace ProyectoCatedraDES.Models;

public class MovimientoCreateVM
{
    [Required] public int ProductoId { get; set; }
    public string? ProductoNombre { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [MaxLength(300)]
    public string? Comentario { get; set; }
}

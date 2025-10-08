using System.ComponentModel.DataAnnotations;

namespace ProyectoCatedraDES.Models;

public class MovimientoCreateVM
{
    [Required(ErrorMessage = "El producto es obligatorio.")]
    public int ProductoId { get; set; }

    public string? ProductoNombre { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [MaxLength(300)]
    public string? Comentario { get; set; }
}

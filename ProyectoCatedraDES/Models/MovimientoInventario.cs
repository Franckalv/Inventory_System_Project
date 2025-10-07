using System.ComponentModel.DataAnnotations;

namespace ProyectoCatedraDES.Models
{
    public enum TipoMovimiento { Entrada = 1, Salida = 2 }

    public class MovimientoInventario
    {
        public int Id { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        public TipoMovimiento Tipo { get; set; }

        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UsuarioId { get; set; }

        [MaxLength(300)]
        public string? Comentario { get; set; }
    }
}

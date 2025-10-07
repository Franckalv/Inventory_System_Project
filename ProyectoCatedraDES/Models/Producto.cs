using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoCatedraDES.Models
{
    [Index(nameof(Codigo), IsUnique = true)]
    public class Producto
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string Codigo { get; set; } = null!;

        [Required, MaxLength(120)]
        public string Nombre { get; set; } = null!;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999)]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue)]
        public int StockActual { get; set; }

        [Range(0, int.MaxValue)]
        public int StockMinimo { get; set; } = 0;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

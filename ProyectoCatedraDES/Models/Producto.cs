using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoCatedraDES.Models
{
    [Index(nameof(Codigo), IsUnique = true)]
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [MaxLength(30, ErrorMessage = "El código no puede tener más de 30 caracteres.")]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(120, ErrorMessage = "El nombre no puede tener más de 120 caracteres.")]
        public string Nombre { get; set; } = null!;

        [MaxLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999, ErrorMessage = "El precio debe estar entre 0 y 9,999,999.")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock actual no puede ser negativo.")]
        public int StockActual { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int StockMinimo { get; set; } = 0;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

namespace ProyectoCatedraDES.Models;

public class DashboardVM
{
    public int TotalProductos { get; set; }
    public int TotalStock { get; set; }
    public int EnAlerta { get; set; }
    public int MovimientosHoy { get; set; }

    public List<MovimientoItem> UltimosMovimientos { get; set; } = new();
    public List<ProductoItem> StockBajo { get; set; } = new();

    public string[] Labels7d { get; set; } = Array.Empty<string>();
    public int[] Entradas7d { get; set; } = Array.Empty<int>();
    public int[] Salidas7d { get; set; } = Array.Empty<int>();
}

public class MovimientoItem
{
    public DateTime Fecha { get; set; }
    public string Producto { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public string Usuario { get; set; } = "";
}

public class ProductoItem
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
}

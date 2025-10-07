using Microsoft.EntityFrameworkCore;
using ProyectoCatedraDES.Data;
using ProyectoCatedraDES.Models;

namespace ProyectoCatedraDES.Services;

public class InventarioService : IInventarioService
{
    private readonly ApplicationDbContext _db;
    public InventarioService(ApplicationDbContext db) => _db = db;

    public async Task RegistrarEntradaAsync(int productoId, int cantidad, string? usuarioId, string? comentario = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser > 0");
        var prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == productoId)
                   ?? throw new InvalidOperationException("Producto no encontrado");

        using var tx = await _db.Database.BeginTransactionAsync();
        prod.StockActual += cantidad;
        _db.Movimientos.Add(new MovimientoInventario
        {
            ProductoId = productoId,
            Cantidad = cantidad,
            Tipo = TipoMovimiento.Entrada,
            UsuarioId = usuarioId,
            Comentario = comentario,
            Fecha = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task RegistrarSalidaAsync(int productoId, int cantidad, string? usuarioId, string? comentario = null)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser > 0");
        var prod = await _db.Productos.FirstOrDefaultAsync(p => p.Id == productoId)
                   ?? throw new InvalidOperationException("Producto no encontrado");

        using var tx = await _db.Database.BeginTransactionAsync();
        if (prod.StockActual < cantidad) throw new InvalidOperationException("Stock insuficiente");

        prod.StockActual -= cantidad;
        _db.Movimientos.Add(new MovimientoInventario
        {
            ProductoId = productoId,
            Cantidad = cantidad,
            Tipo = TipoMovimiento.Salida,
            UsuarioId = usuarioId,
            Comentario = comentario,
            Fecha = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}

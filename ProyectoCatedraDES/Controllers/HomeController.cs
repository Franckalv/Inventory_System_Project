using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCatedraDES.Data;
using ProyectoCatedraDES.Models;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.UtcNow.Date;
        var hace7 = hoy.AddDays(-6);

        var vm = new DashboardVM
        {
            TotalProductos = await _db.Productos.CountAsync(),
            TotalStock = await _db.Productos.SumAsync(p => (int?)p.StockActual) ?? 0,
            EnAlerta = await _db.Productos.CountAsync(p => p.StockActual <= p.StockMinimo),
            MovimientosHoy = await _db.Movimientos.CountAsync(m => m.Fecha >= hoy && m.Fecha < hoy.AddDays(1)),
            UltimosMovimientos = await _db.Movimientos
                .AsNoTracking().Include(m => m.Producto)
                .OrderByDescending(m => m.Fecha)
                .Take(10)
                .Select(m => new MovimientoItem
                {
                    Fecha = m.Fecha,
                    Producto = m.Producto!.Nombre,
                    Tipo = m.Tipo.ToString(),
                    Cantidad = m.Cantidad,
                    Usuario = m.UsuarioId ?? ""
                })
                .ToListAsync(),
            StockBajo = await _db.Productos
                .AsNoTracking()
                .Where(p => p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.StockActual - p.StockMinimo)
                .Take(10)
                .Select(p => new ProductoItem
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    StockActual = p.StockActual,
                    StockMinimo = p.StockMinimo
                })
                .ToListAsync()
        };

        var movimientos7d = await _db.Movimientos
            .AsNoTracking()
            .Where(m => m.Fecha >= hace7 && m.Fecha < hoy.AddDays(1))
            .ToListAsync();

        var labels = Enumerable.Range(0, 7)
            .Select(i => hace7.AddDays(i))
            .Select(d => d.ToLocalTime().ToString("dd/MM"))
            .ToArray();

        int[] entradas = new int[7];
        int[] salidas = new int[7];

        foreach (var m in movimientos7d)
        {
            var dia = m.Fecha.Date;
            int idx = (int)(dia - hace7).TotalDays;
            if (idx is >= 0 and < 7)
            {
                if (m.Tipo == TipoMovimiento.Entrada) entradas[idx] += m.Cantidad;
                else salidas[idx] += m.Cantidad;
            }
        }

        vm.Labels7d = labels;
        vm.Entradas7d = entradas;
        vm.Salidas7d = salidas;

        return View(vm);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCatedraDES.Data;

namespace ProyectoCatedraDES.Controllers;

[Authorize(Roles = "Admin,Operador")]
public class AlertasController : Controller
{
    private readonly ApplicationDbContext _db;
    public AlertasController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var lista = await _db.Productos
            .Where(p => p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.StockActual - p.StockMinimo)
            .AsNoTracking()
            .ToListAsync();

        return View(lista);
    }
}

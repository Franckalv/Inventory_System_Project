using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCatedraDES.Data;
using ProyectoCatedraDES.Models;
using ProyectoCatedraDES.Services;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;

namespace ProyectoCatedraDES.Controllers;

[Authorize(Roles = "Admin,Operador")]
public class MovimientosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IInventarioService _svc;
    private readonly UserManager<IdentityUser> _userMgr;

    public MovimientosController(ApplicationDbContext db, IInventarioService svc, UserManager<IdentityUser> userMgr)
    { _db = db; _svc = svc; _userMgr = userMgr; }

    // LISTADO
    public async Task<IActionResult> Index(int? productoId)
    {
        var q = _db.Movimientos.Include(m => m.Producto).AsQueryable();
        if (productoId.HasValue) q = q.Where(m => m.ProductoId == productoId);
        var data = await q.OrderByDescending(m => m.Fecha).Take(200).ToListAsync();
        return View(data);
    }

    // ENTRADA
    public async Task<IActionResult> CrearEntrada(int productoId)
    {
        var p = await _db.Productos.FindAsync(productoId);
        if (p is null) return NotFound();
        return View(new MovimientoCreateVM { ProductoId = p.Id, ProductoNombre = p.Nombre });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearEntrada(MovimientoCreateVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var userId = _userMgr.GetUserId(User);
        await _svc.RegistrarEntradaAsync(vm.ProductoId, vm.Cantidad, userId, vm.Comentario);
        TempData["Ok"] = "Entrada registrada.";
        return RedirectToAction(nameof(Index), new { productoId = vm.ProductoId });
    }

    // SALIDA
    public async Task<IActionResult> CrearSalida(int productoId)
    {
        var p = await _db.Productos.FindAsync(productoId);
        if (p is null) return NotFound();
        return View(new MovimientoCreateVM { ProductoId = p.Id, ProductoNombre = p.Nombre });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearSalida(MovimientoCreateVM vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var userId = _userMgr.GetUserId(User);
            await _svc.RegistrarSalidaAsync(vm.ProductoId, vm.Cantidad, userId, vm.Comentario);
            TempData["Ok"] = "Salida registrada.";
            return RedirectToAction(nameof(Index), new { productoId = vm.ProductoId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ModelState.AddModelError(nameof(MovimientoCreateVM.Cantidad), ex.Message);
            return View(vm);
        }

    }

    // REPORTES

    // EXCEL (opcionalmente filtrado por producto)
    [HttpGet]
    public async Task<IActionResult> ExportarExcel(int? productoId)
    {
        IQueryable<MovimientoInventario> q = _db.Movimientos.AsNoTracking();

        if (productoId.HasValue)
            q = q.Where(m => m.ProductoId == productoId.Value);

        var lista = await q
            .Include(m => m.Producto)
            .OrderBy(m => m.Fecha)
            .ToListAsync();

        using var wb = new ClosedXML.Excel.XLWorkbook();
        var ws = wb.Worksheets.Add("Movimientos");

        ws.Cell(1, 1).Value = "Fecha";
        ws.Cell(1, 2).Value = "Producto";
        ws.Cell(1, 3).Value = "Tipo";
        ws.Cell(1, 4).Value = "Cantidad";
        ws.Cell(1, 5).Value = "Usuario";
        ws.Cell(1, 6).Value = "Comentario";
        ws.Range("A1:F1").Style.Font.Bold = true;

        var r = 2;
        foreach (var m in lista)
        {
            ws.Cell(r, 1).Value = m.Fecha.ToLocalTime();
            ws.Cell(r, 2).Value = m.Producto?.Nombre;
            ws.Cell(r, 3).Value = m.Tipo.ToString();
            ws.Cell(r, 4).Value = m.Cantidad;
            ws.Cell(r, 5).Value = m.UsuarioId;
            ws.Cell(r, 6).Value = m.Comentario;
            r++;
        }
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        var nombre = productoId.HasValue ? $"Movimientos_{productoId}.xlsx" : "Movimientos.xlsx";
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            nombre);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdf(int? productoId)
    {
        IQueryable<MovimientoInventario> q = _db.Movimientos.AsNoTracking();

        if (productoId.HasValue)
            q = q.Where(m => m.ProductoId == productoId.Value);

        var lista = await q
            .Include(m => m.Producto)
            .OrderBy(m => m.Fecha)
            .ToListAsync();

        var now = DateTime.Now;

        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);

                page.Header().Row(row =>
                {
                    row.RelativeItem().Text("Distribuidora Don Bosco")
                       .SemiBold().FontSize(14);
                    var titulo = productoId.HasValue
                        ? $"Movimientos del producto #{productoId}"
                        : "Movimientos";
                    row.ConstantItem(300).AlignRight().Text($"{titulo} · {now:g}");
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(3); // Fecha
                        c.RelativeColumn(4); // Producto
                        c.RelativeColumn(2); // Tipo
                        c.RelativeColumn(2); // Cantidad
                        c.RelativeColumn(4); // Usuario
                        c.RelativeColumn(6); // Comentario
                    });

                    table.Header(h =>
                    {
                        h.Cell().Text("Fecha").SemiBold();
                        h.Cell().Text("Producto").SemiBold();
                        h.Cell().Text("Tipo").SemiBold();
                        h.Cell().Text("Cantidad").SemiBold();
                        h.Cell().Text("Usuario").SemiBold();
                        h.Cell().Text("Comentario").SemiBold();
                    });

                    foreach (var m in lista)
                    {
                        table.Cell().Text(m.Fecha.ToLocalTime().ToString("g"));
                        table.Cell().Text(m.Producto?.Nombre);
                        table.Cell().Text(m.Tipo.ToString());
                        table.Cell().Text(m.Cantidad.ToString());
                        table.Cell().Text(m.UsuarioId);
                        table.Cell().Text(m.Comentario);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página "); x.CurrentPageNumber();
                    x.Span(" / "); x.TotalPages();
                });
            });
        });

        var bytes = doc.GeneratePdf();
        var nombre = productoId.HasValue ? $"Movimientos_{productoId}.pdf" : "Movimientos.pdf";
        return File(bytes, "application/pdf", nombre);
    }
}

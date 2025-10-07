using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCatedraDES.Data;
using ProyectoCatedraDES.Models;
using ClosedXML.Excel;
using System.IO;                
using QuestPDF.Fluent;          
using QuestPDF.Helpers;         
using QuestPDF.Infrastructure;  

namespace ProyectoCatedraDES.Controllers
{
    [Authorize(Roles = "Admin,Operador")]
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductosController(ApplicationDbContext context) => _context = context;

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            var items = await _context.Productos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return View(items);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();

            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto is null) return NotFound();

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create() => View();

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Descripcion,Precio,StockActual,StockMinimo")] Producto producto)
        {
            // Validación de código único
            if (await _context.Productos.AnyAsync(p => p.Codigo == producto.Codigo))
                ModelState.AddModelError(nameof(Producto.Codigo), "El código ya existe.");

            if (!ModelState.IsValid) return View(producto);

            _context.Add(producto);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto is null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Edit/5 (con control de concurrencia RowVersion)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Codigo,Nombre,Descripcion,Precio,StockActual,StockMinimo,RowVersion")] Producto form)
        {
            if (id != form.Id) return NotFound();

            if (await _context.Productos.AnyAsync(p => p.Codigo == form.Codigo && p.Id != form.Id))
                ModelState.AddModelError(nameof(Producto.Codigo), "El código ya existe.");

            if (!ModelState.IsValid) return View(form);

            var entity = await _context.Productos.FirstOrDefaultAsync(p => p.Id == form.Id);
            if (entity is null) return NotFound();

            entity.Codigo = form.Codigo;
            entity.Nombre = form.Nombre;
            entity.Descripcion = form.Descripcion;
            entity.Precio = form.Precio;
            entity.StockActual = form.StockActual;
            entity.StockMinimo = form.StockMinimo;

            _context.Entry(entity).Property(nameof(Producto.RowVersion)).OriginalValue = form.RowVersion!;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Producto actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "El registro fue modificado por otro usuario. Vuelve a intentarlo.");
                var dbValues = await _context.Productos.AsNoTracking().FirstAsync(p => p.Id == form.Id);
                form.RowVersion = dbValues.RowVersion;
                return View(form);
            }
        }

        // GET: Productos/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto is null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == id);
            if (producto is not null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Producto eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id) => _context.Productos.Any(e => e.Id == id);

        // REPORTES

        // EXCEL: /Productos/ExportarExcel
        [HttpGet]
        public async Task<IActionResult> ExportarExcel()
        {
            var data = await _context.Productos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.StockActual,
                    p.StockMinimo
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Productos");

            // Encabezados
            ws.Cell(1, 1).Value = "Código";
            ws.Cell(1, 2).Value = "Nombre";
            ws.Cell(1, 3).Value = "Descripción";
            ws.Cell(1, 4).Value = "Precio";
            ws.Cell(1, 5).Value = "Stock";
            ws.Cell(1, 6).Value = "Mínimo";
            ws.Range("A1:F1").Style.Font.Bold = true;

            // Datos
            var r = 2;
            foreach (var p in data)
            {
                ws.Cell(r, 1).Value = p.Codigo;
                ws.Cell(r, 2).Value = p.Nombre;
                ws.Cell(r, 3).Value = p.Descripcion;
                ws.Cell(r, 4).Value = p.Precio;
                ws.Cell(r, 5).Value = p.StockActual;
                ws.Cell(r, 6).Value = p.StockMinimo;
                r++;
            }

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Productos.xlsx");
        }

        // PDF: /Productos/ExportarPdf
        [HttpGet]
        public async Task<IActionResult> ExportarPdf()
        {
            var lista = await _context.Productos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var now = DateTime.Now;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Text("Distribuidora Don Bosco")
                           .SemiBold().FontSize(14);
                        row.ConstantItem(250).AlignRight().Text($"Productos · {now:g}");
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2); // Código
                            c.RelativeColumn(4); // Nombre
                            c.RelativeColumn(6); // Descripción
                            c.RelativeColumn(2); // Precio
                            c.RelativeColumn(2); // Stock
                            c.RelativeColumn(2); // Mínimo
                        });

                        table.Header(h =>
                        {
                            h.Cell().Text("Código").SemiBold();
                            h.Cell().Text("Nombre").SemiBold();
                            h.Cell().Text("Descripción").SemiBold();
                            h.Cell().Text("Precio").SemiBold();
                            h.Cell().Text("Stock").SemiBold();
                            h.Cell().Text("Mínimo").SemiBold();
                        });

                        foreach (var p in lista)
                        {
                            table.Cell().Text(p.Codigo);
                            table.Cell().Text(p.Nombre);
                            table.Cell().Text(p.Descripcion);
                            table.Cell().Text($"{p.Precio:N2}");
                            table.Cell().Text(p.StockActual.ToString());
                            table.Cell().Text(p.StockMinimo.ToString());
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            var bytes = doc.GeneratePdf();
            return File(bytes, "application/pdf", "Productos.pdf");
        }
    }
}

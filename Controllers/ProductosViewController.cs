using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LOGIN.Data;
using LOGIN.Models;

namespace LOGIN.Controllers
{
    public class ProductosViewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosViewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductosView (CON BÚSQUEDA Y FILTROS)
        public async Task<IActionResult> Index(string searchString, decimal? precioMin, decimal? precioMax)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            // Consulta base
            var productos = from p in _context.Productos
                            select p;

            // 🔍 BÚSQUEDA POR NOMBRE (insensible a mayúsculas/minúsculas)
            if (!string.IsNullOrEmpty(searchString))
            {
                productos = productos.Where(p => p.Nombre.Contains(searchString));
            }

            // 💰 FILTRO POR PRECIO MÍNIMO
            if (precioMin.HasValue)
            {
                productos = productos.Where(p => p.Precio >= precioMin.Value);
            }

            // 💰 FILTRO POR PRECIO MÁXIMO
            if (precioMax.HasValue)
            {
                productos = productos.Where(p => p.Precio <= precioMax.Value);
            }

            // Enviar valores al ViewBag para mantener los filtros en la vista
            ViewBag.SearchString = searchString;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;

            return View(await productos.ToListAsync());
        }

        // GET: ProductosView/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
                return NotFound();

            var producto = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // GET: ProductosView/Create
        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        // POST: ProductosView/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                producto.FechaRegistro = DateTime.Now;
                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Producto creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: ProductosView/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
                return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // POST: ProductosView/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != producto.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existente = await _context.Productos.FindAsync(id);
                    if (existente == null)
                        return NotFound();

                    existente.Nombre = producto.Nombre;
                    existente.Descripcion = producto.Descripcion;
                    existente.Cantidad = producto.Cantidad;
                    existente.Precio = producto.Precio;

                    _context.Update(existente);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Producto actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: ProductosView/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")))
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
                return NotFound();

            var producto = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // POST: ProductosView/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Producto eliminado exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
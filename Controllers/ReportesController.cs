using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LOGIN.Data;
using LOGIN.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LOGIN.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Verifica si el usuario es administrador o está logueado (puedes adaptarlo a tus roles)
        private bool UsuarioLogueado()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        // GET: Reportes/Ventas
        public async Task<IActionResult> Ventas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!UsuarioLogueado())
                return RedirectToAction("Login", "Account");

            // Rango de fechas por defecto: últimos 30 días
            DateTime inicio = fechaInicio ?? DateTime.Now.AddDays(-30);
            DateTime fin = fechaFin ?? DateTime.Now;

            // Ajustar el fin de día a las 23:59:59
            DateTime finAjustado = fin.Date.AddDays(1).AddTicks(-1);

            // Consulta base de pedidos dentro del rango y que no estén cancelados
            var queryPedidos = _context.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles!)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.FechaPedido >= inicio && p.FechaPedido <= finAjustado);

            var pedidosLista = await queryPedidos.OrderByDescending(p => p.FechaPedido).ToListAsync();

            // Procesar KPI's principales en memoria
            decimal totalIngresos = pedidosLista.Where(p => p.Estado != EstadoPedido.Cancelado).Sum(p => p.Total);
            int totalPedidos = pedidosLista.Count;
            int productosVendidos = pedidosLista.Where(p => p.Estado != EstadoPedido.Cancelado)
                                                .SelectMany(p => p.Detalles!)
                                                .Sum(d => d.Cantidad);

            // Obtener los productos más vendidos
            var topProductos = pedidosLista.Where(p => p.Estado != EstadoPedido.Cancelado)
                .SelectMany(p => p.Detalles!)
                .GroupBy(d => d.Producto!.Nombre)
                .Select(g => new TopProductoViewModel
                {
                    ProductoNombre = g.Key,
                    CantidadVendida = g.Sum(x => x.Cantidad),
                    TotalRecaudado = g.Sum(x => x.Subtotal)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(5)
                .ToList();

            // Construir el modelo final
            var viewModel = new ReporteVentasViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin,
                TotalIngresos = totalIngresos,
                TotalPedidos = totalPedidos,
                ProductosVendidos = productosVendidos,
                PedidosRecientes = pedidosLista,
                TopProductos = topProductos
            };

            return View(viewModel);
        }
    }
}
using System;
using System.Collections.Generic;

namespace LOGIN.Models
{
    public class ReporteVentasViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal TotalIngresos { get; set; }
        public int TotalPedidos { get; set; }
        public int ProductosVendidos { get; set; }

        // Listado de pedidos en el rango de fechas
        public List<Pedido> PedidosRecientes { get; set; } = new List<Pedido>();

        // Agrupación para saber qué productos son los más populares
        public List<TopProductoViewModel> TopProductos { get; set; } = new List<TopProductoViewModel>();
    }

    public class TopProductoViewModel
    {
        public string? ProductoNombre { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalRecaudado { get; set; }
    }
}
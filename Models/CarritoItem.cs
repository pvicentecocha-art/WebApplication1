namespace LOGIN.Models
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public int UsuarioId { get; set; }
        public int ProductoId { get; set; }

        // Navegación
        public Usuario? Usuario { get; set; }
        public Producto? Producto { get; set; }
    }
}
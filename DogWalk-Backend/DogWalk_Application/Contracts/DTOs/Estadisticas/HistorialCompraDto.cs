namespace DogWalk_Application.Contracts.DTOs.Estadisticas
{
    /// <summary>
    /// DTO para representar el historial de compras de un usuario.
    /// </summary>
    public class HistorialCompraDto
    {
        public Guid Id { get; set; }
        public DateTime FechaCompra { get; set; }
        public string NumeroFactura { get; set; }
        public decimal Total { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; } = new List<DetalleCompraDto>();
    }

    /// <summary>
    /// DTO para representar un detalle de compra.
    /// </summary>
    public class DetalleCompraDto
    {
        public string NombreArticulo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
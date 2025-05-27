using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    /// <summary>
    /// DTO para representar un artículo en el carrito.
    /// </summary>
    public class ItemCarritoDto
    {
        public Guid Id { get; set; }
        public Guid ArticuloId { get; set; }
        public string NombreArticulo { get; set; }
        public string ImagenUrl { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}

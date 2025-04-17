using System;

namespace DogWalk_Application.Contracts.DTOs.Carrito
{
    public class ItemCarritoDto
    {
        public Guid Id { get; set; }
        public string TipoItem { get; set; } // "Articulo" o "Servicio"
        public Guid ItemId { get; set; }
        public string NombreItem { get; set; }
        public string ImagenUrl { get; set; } // Imagen del artículo o servicio
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}

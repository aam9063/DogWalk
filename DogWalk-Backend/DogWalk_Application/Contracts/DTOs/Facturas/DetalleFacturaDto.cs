using System;

namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    public class DetalleFacturaDto
    {
        public Guid Id { get; set; }
        public Guid FacturaId { get; set; }
        public string TipoItem { get; set; } // "Articulo" o "Servicio"
        public Guid ItemId { get; set; }
        public string NombreItem { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}

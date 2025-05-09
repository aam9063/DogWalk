using System;

namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    public class DetalleFacturaDto
    {
        public Guid Id { get; set; }
        public Guid ArticuloId { get; set; }
        public string NombreArticulo { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}

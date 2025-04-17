using System;

namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    public class FacturaResumenDto
    {
        public Guid Id { get; set; }
        public DateTime FechaFactura { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public int CantidadItems { get; set; }
    }
}


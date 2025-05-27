using System;

namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    /// <summary>
    /// DTO para generar un PDF de factura.
    /// </summary>
    public class GenerarFacturaPdfDto
    {
        public Guid FacturaId { get; set; }
        public bool EnviarEmail { get; set; } = false;
    }
}

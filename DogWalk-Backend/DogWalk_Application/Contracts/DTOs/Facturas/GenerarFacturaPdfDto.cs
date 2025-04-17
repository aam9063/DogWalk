using System;

namespace DogWalk_Application.Contracts.DTOs.Facturas
{
    public class GenerarFacturaPdfDto
    {
        public Guid FacturaId { get; set; }
        public bool EnviarEmail { get; set; } = false;
    }
}

using System;

namespace DogWalk_Application.Contracts.DTOs.Precios
{
    public class ActualizarPrecioDto
    {
        public Guid PaseadorId { get; set; }
        public Guid ServicioId { get; set; }
        public decimal Precio { get; set; }
    }
}

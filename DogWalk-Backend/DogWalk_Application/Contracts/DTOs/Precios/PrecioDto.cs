using System;

namespace DogWalk_Application.Contracts.DTOs.Precios
{
    public class PrecioDto
    {
        public Guid Id { get; set; }
        public Guid PaseadorId { get; set; }
        public Guid ServicioId { get; set; }
        public string NombreServicio { get; set; }
        public string DescripcionServicio { get; set; }
        public decimal Precio { get; set; }
    }
}

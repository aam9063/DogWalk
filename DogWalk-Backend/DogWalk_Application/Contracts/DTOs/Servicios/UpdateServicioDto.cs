using System;

namespace DogWalk_Application.Contracts.DTOs.Servicios
{
    public class UpdateServicioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioReferencia { get; set; }
    }
}

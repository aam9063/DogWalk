using System;

namespace DogWalk_Application.Contracts.DTOs.Servicios
{   
    /// <summary>
    /// DTO para actualizar un servicio.
    /// </summary>
    public class UpdateServicioDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioReferencia { get; set; }
    }
}

using System;

namespace DogWalk_Application.Contracts.DTOs.Precios
{
    /// <summary>
    /// DTO para representar un precio.
    /// </summary>
    public class PrecioDto
    {
        public Guid Id { get; set; }
        public Guid PaseadorId { get; set; }
        public Guid ServicioId { get; set; }
        public string NombreServicio { get; set; }
        public string DescripcionServicio { get; set; }
        public decimal Precio { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un precio.
    /// </summary>
    public class ActualizarPrecioDto
    {
        public Guid PaseadorId { get; set; }
        public Guid ServicioId { get; set; }
        public decimal Precio { get; set; }
    }
}

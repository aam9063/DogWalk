using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    /// <summary>
    /// DTO para representar la disponibilidad horaria de un paseador.
    /// </summary>
    public class DisponibilidadHorariaDto
    {
        public Guid Id { get; set; }
        public Guid PaseadorId { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } // "Disponible" o "Reservado"
    }
}

using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    /// <summary>
    /// DTO para actualizar la disponibilidad de un paseador.
    /// </summary>
    public class ActualizarDisponibilidadDto
    {
        public Guid DisponibilidadId { get; set; }
        public string Estado { get; set; } // "Disponible" o "Reservado"
    }
}
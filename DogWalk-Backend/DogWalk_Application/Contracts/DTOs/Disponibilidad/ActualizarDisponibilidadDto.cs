using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    public class ActualizarDisponibilidadDto
    {
        public Guid DisponibilidadId { get; set; }
        public string Estado { get; set; } // "Disponible" o "Reservado"
    }
}
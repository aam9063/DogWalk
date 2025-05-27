using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    /// <summary>
    /// DTO para crear una nueva disponibilidad.
    /// </summary>
    public class CrearDisponibilidadDto
    {
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public int IntervaloMinutos { get; set; } = 60; // Intervalo en minutos entre cada slot
    }
}
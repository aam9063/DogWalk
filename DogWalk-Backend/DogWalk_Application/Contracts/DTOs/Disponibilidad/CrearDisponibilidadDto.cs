using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    public class CrearDisponibilidadDto
    {
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public int IntervaloMinutos { get; set; } = 60; // Intervalo en minutos entre cada slot
    }
}
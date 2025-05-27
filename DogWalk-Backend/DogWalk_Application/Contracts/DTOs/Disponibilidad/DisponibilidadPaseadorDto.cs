using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    /// <summary>
    /// DTO para representar la disponibilidad de un paseador.
    /// </summary>
    public class DisponibilidadPaseadorDto
    {
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public List<DisponibilidadDiaDto> Dias { get; set; } = new List<DisponibilidadDiaDto>();
    }

    /// <summary>
    /// DTO para representar la disponibilidad de un día.
    /// </summary>
    public class DisponibilidadDiaDto
    {
        public DateTime Fecha { get; set; }
        public List<DisponibilidadHoraDto> Horas { get; set; } = new List<DisponibilidadHoraDto>();
    }

    /// <summary>
    /// DTO para representar la disponibilidad de una hora.
    /// </summary>
    public class DisponibilidadHoraDto
    {
        public Guid Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; }
    }
}

using System;

namespace DogWalk_Application.Contracts.DTOs.Disponibilidad
{
    public class DisponibilidadPaseadorDto
    {
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public List<DisponibilidadDiaDto> Dias { get; set; } = new List<DisponibilidadDiaDto>();
    }

    public class DisponibilidadDiaDto
    {
        public DateTime Fecha { get; set; }
        public List<DisponibilidadHoraDto> Horas { get; set; } = new List<DisponibilidadHoraDto>();
    }

    public class DisponibilidadHoraDto
    {
        public Guid Id { get; set; }
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; }
    }
}

using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    public class OpinionPerroDto
    {
        public Guid Id { get; set; }
        public Guid PerroId { get; set; }
        public string NombrePerro { get; set; }
        public string FotoPerro { get; set; }
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaOpinion { get; set; }
    }
}

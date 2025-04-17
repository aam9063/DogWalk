using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    public class CrearRankingPaseadorDto
    {
        public Guid PaseadorId { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
    }
}

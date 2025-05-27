using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    /// <summary>
    /// DTO para crear un nuevo ranking de paseador.
    /// </summary>
    public class CrearRankingPaseadorDto
    {
        public Guid PaseadorId { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
    }
}

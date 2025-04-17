using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    public class RankingPaseadorDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string FotoUsuario { get; set; }
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
        public DateTime FechaValoracion { get; set; }
    }
}
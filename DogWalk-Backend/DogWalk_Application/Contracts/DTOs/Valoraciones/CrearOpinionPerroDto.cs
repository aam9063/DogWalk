using System;

namespace DogWalk_Application.Contracts.DTOs.Valoraciones
{
    public class CrearOpinionPerroDto
    {
        public Guid PerroId { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
    }
}

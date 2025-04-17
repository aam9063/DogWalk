using System;
using System.Collections.Generic;

namespace DogWalk_Application.Contracts.DTOs.Perros
{
    public class PerroWithOpinionesDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Raza { get; set; }
        public int Edad { get; set; }
        public string GpsUbicacion { get; set; }
        public decimal ValoracionPromedio { get; set; }
        public List<string> UrlsFotos { get; set; } = new List<string>();
        public List<OpinionPerroDto> Opiniones { get; set; } = new List<OpinionPerroDto>();
    }

    public class OpinionPerroDto
    {
        public Guid Id { get; set; }
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public string FotoPaseador { get; set; }
        public int Puntuacion { get; set; }
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }
}
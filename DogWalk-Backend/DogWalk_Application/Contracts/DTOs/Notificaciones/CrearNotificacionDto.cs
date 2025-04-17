using System;

namespace DogWalk_Application.Contracts.DTOs.Notificaciones
{
    public class CrearNotificacionDto
    {
        public Guid UsuarioId { get; set; }
        public string Tipo { get; set; }
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string RutaAccion { get; set; }
    }
}

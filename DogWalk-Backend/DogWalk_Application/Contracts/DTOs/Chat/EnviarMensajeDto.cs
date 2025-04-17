using System;

namespace DogWalk_Application.Contracts.DTOs.Chat
{
    public class EnviarMensajeDto
    {
        public Guid DestinatarioId { get; set; }
        public string TipoDestinatario { get; set; } // "Usuario" o "Paseador"
        public string Mensaje { get; set; }
    }
}

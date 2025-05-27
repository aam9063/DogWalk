using System;

namespace DogWalk_Application.Contracts.DTOs.Chat
{
    /// <summary>
    /// DTO para enviar un mensaje de chat.
    /// </summary>
    public class EnviarMensajeDto
    {
        public Guid EnviadorId { get; set; }
        public string TipoEmisor { get; set; } // "Usuario" o "Paseador"
        public Guid DestinatarioId { get; set; }
        public string TipoDestinatario { get; set; } // "Usuario" o "Paseador"
        public string Mensaje { get; set; }
    }
}

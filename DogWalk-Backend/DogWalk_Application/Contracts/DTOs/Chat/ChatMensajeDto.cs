using System;

namespace DogWalk_Application.Contracts.DTOs.Chat
{
    public class ChatMensajeDto
    {
        public Guid Id { get; set; }
        public Guid EnviadorId { get; set; }
        public string NombreEnviador { get; set; }
        public string FotoEnviador { get; set; }
        public string TipoEnviador { get; set; } // "Usuario" o "Paseador"
        public string Mensaje { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Leido { get; set; }
    }
}

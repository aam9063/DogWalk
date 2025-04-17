using System;

namespace DogWalk_Application.Contracts.DTOs.Chat
{
    public class ConversacionDto
    {
        public Guid UsuarioId { get; set; }
        public string NombreUsuario { get; set; }
        public string FotoUsuario { get; set; }
        public Guid PaseadorId { get; set; }
        public string NombrePaseador { get; set; }
        public string FotoPaseador { get; set; }
        public List<ChatMensajeDto> Mensajes { get; set; } = new List<ChatMensajeDto>();
    }
}

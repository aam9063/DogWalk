using System;

namespace DogWalk_Application.Contracts.DTOs.Chat
{
    /// <summary>
    /// DTO para representar un resumen de una conversación.
    /// </summary>
    public class ResumenConversacionDto
    {
        public Guid UsuarioId { get; set; }
        public Guid PaseadorId { get; set; }
        public string NombreContacto { get; set; } // Nombre del Usuario o Paseador según corresponda
        public string FotoContacto { get; set; }
        public string UltimoMensaje { get; set; }
        public DateTime FechaUltimoMensaje { get; set; }
        public int MensajesNoLeidos { get; set; }
    }
}

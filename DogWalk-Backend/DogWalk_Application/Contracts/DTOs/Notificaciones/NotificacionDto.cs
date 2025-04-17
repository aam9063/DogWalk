using System;

namespace DogWalk_Application.Contracts.DTOs.Notificaciones
{
    public class NotificacionDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string Tipo { get; set; } // "Reserva", "Mensaje", "Sistema", etc.
        public string Titulo { get; set; }
        public string Mensaje { get; set; }
        public string RutaAccion { get; set; } // Ruta para navegar cuando se hace clic en la notificación
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
    }
}

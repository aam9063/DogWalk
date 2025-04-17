using System;

namespace DogWalk_Domain.Entities;

 public class ChatMensaje : EntityBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid PaseadorId { get; private set; }
        public string Mensaje { get; private set; }
        public DateTime FechaHora { get; private set; }
        public bool LeidoPorUsuario { get; private set; }
        public bool LeidoPorPaseador { get; private set; }
        
        // Relaciones
        public Usuario Usuario { get; private set; }
        public Paseador Paseador { get; private set; }
        
        private ChatMensaje() : base() { } // Para EF Core
        
        public ChatMensaje(
            Guid id,
            Guid usuarioId,
            Guid paseadorId,
            string mensaje
        ) : base(id)
        {
            UsuarioId = usuarioId;
            PaseadorId = paseadorId;
            Mensaje = mensaje;
            FechaHora = DateTime.UtcNow;
            LeidoPorUsuario = false;
            LeidoPorPaseador = false;
        }
        
        public void MarcarComoLeidoUsuario()
        {
            LeidoPorUsuario = true;
            ActualizarFechaModificacion();
        }
        
        public void MarcarComoLeidoPaseador()
        {
            LeidoPorPaseador = true;
            ActualizarFechaModificacion();
        }
    }

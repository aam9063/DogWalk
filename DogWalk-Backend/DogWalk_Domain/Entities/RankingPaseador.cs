using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain.Entities;

 public class RankingPaseador : EntityBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid PaseadorId { get; private set; }
        public Valoracion Valoracion { get; private set; }
        public string Comentario { get; private set; }
        
        // Relaciones
        public Usuario Usuario { get; private set; }
        public Paseador Paseador { get; private set; }
        
        private RankingPaseador() : base() { } 
        
        public RankingPaseador(
            Guid id,
            Guid usuarioId,
            Guid paseadorId,
            Valoracion valoracion,
            string comentario = null
        ) : base(id)
        {
            UsuarioId = usuarioId;
            PaseadorId = paseadorId;
            Valoracion = valoracion;
            Comentario = comentario;
        }
        
        public void ActualizarValoracion(Valoracion valoracion, string comentario = null)
        {
            Valoracion = valoracion;
            
            if (comentario != null)
            {
                Comentario = comentario;
            }
            
            ActualizarFechaModificacion();
        }
    }

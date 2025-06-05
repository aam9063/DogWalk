using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain.Entities;

 public class OpinionPerro : EntityBase
    {
        public Guid PerroId { get; private set; }
        public Guid PaseadorId { get; private set; }
        public Valoracion Valoracion { get; private set; }
        public string Comentario { get; private set; }
        
        // Relaciones
        public Perro Perro { get; private set; }
        public Paseador Paseador { get; private set; }
        
        private OpinionPerro() : base() { } 
        
        public OpinionPerro(
            Guid id,
            Guid perroId,
            Guid paseadorId,
            Valoracion valoracion,
            string comentario = null
        ) : base(id)
        {
            PerroId = perroId;
            PaseadorId = paseadorId;
            Valoracion = valoracion;
            Comentario = comentario;
        }
        
        public void ActualizarOpinion(Valoracion valoracion, string comentario = null)
        {
            Valoracion = valoracion;
            
            if (comentario != null)
            {
                Comentario = comentario;
            }
            
            ActualizarFechaModificacion();
        }
    }
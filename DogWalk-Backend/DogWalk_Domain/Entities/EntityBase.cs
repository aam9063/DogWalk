using System;

namespace DogWalk_Domain.Entities;

public abstract class EntityBase
    {
        public Guid Id { get; protected set; }
        public DateTime CreadoEn { get; protected set; }
        public DateTime? ModificadoEn { get; protected set; }
        
        protected EntityBase()
        {
            Id = Guid.NewGuid();
            CreadoEn = DateTime.UtcNow;
        }
        
        protected EntityBase(Guid id)
        {
            Id = id;
            CreadoEn = DateTime.UtcNow;
        }
        
        public void ActualizarFechaModificacion()
        {
            ModificadoEn = DateTime.UtcNow;
        }
    }

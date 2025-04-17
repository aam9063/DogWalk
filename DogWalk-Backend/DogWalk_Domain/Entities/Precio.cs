using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain.Entities;

public class Precio : EntityBase
    {
        public Guid PaseadorId { get; private set; }
        public Guid ServicioId { get; private set; }
        public Dinero Valor { get; private set; }
        
        // Relaciones
        public Paseador Paseador { get; private set; }
        public Servicio Servicio { get; private set; }
        
        private Precio() : base() { } // Para EF Core
        
        public Precio(
            Guid id,
            Guid paseadorId,
            Guid servicioId,
            Dinero valor
        ) : base(id)
        {
            PaseadorId = paseadorId;
            ServicioId = servicioId;
            Valor = valor;
        }
        
        public void ActualizarPrecio(Dinero nuevoPrecio)
        {
            Valor = nuevoPrecio;
            ActualizarFechaModificacion();
        }
    }

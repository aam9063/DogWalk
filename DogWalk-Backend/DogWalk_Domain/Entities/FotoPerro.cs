using System;

namespace DogWalk_Domain.Entities;

public class FotoPerro : EntityBase
    {
        public Guid PerroId { get; private set; }
        public string UrlFoto { get; private set; }
        public string Descripcion { get; private set; }
        
        // Relaciones
        public Perro Perro { get; private set; }
        
        private FotoPerro() : base() { } // Para EF Core
        
        public FotoPerro(
            Guid id,
            Guid perroId,
            string urlFoto,
            string descripcion = null
        ) : base(id)
        {
            PerroId = perroId;
            UrlFoto = urlFoto;
            Descripcion = descripcion;
        }
        
        public void ActualizarDescripcion(string descripcion)
        {
            Descripcion = descripcion;
            ActualizarFechaModificacion();
        }
    }

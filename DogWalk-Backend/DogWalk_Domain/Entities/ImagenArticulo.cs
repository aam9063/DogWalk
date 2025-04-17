using System;

namespace DogWalk_Domain.Entities;

public class ImagenArticulo : EntityBase
    {
        public Guid ArticuloId { get; private set; }
        public string UrlImagen { get; private set; }
        public bool EsPrincipal { get; private set; }
        
        // Relaciones
        public Articulo Articulo { get; private set; }
        
        private ImagenArticulo() : base() { } // Para EF Core
        
        public ImagenArticulo(
            Guid id,
            Guid articuloId,
            string urlImagen,
            bool esPrincipal = false
        ) : base(id)
        {
            ArticuloId = articuloId;
            UrlImagen = urlImagen;
            EsPrincipal = esPrincipal;
        }
        
        public void EstablecerComoPrincipal()
        {
            EsPrincipal = true;
            ActualizarFechaModificacion();
        }
        
        public void QuitarComoPrincipal()
        {
            EsPrincipal = false;
            ActualizarFechaModificacion();
        }
    }
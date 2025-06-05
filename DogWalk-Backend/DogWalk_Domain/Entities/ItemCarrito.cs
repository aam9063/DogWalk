using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain.Entities;

public class ItemCarrito : EntityBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid ArticuloId { get; private set; }  
        public int Cantidad { get; private set; }
        public Dinero PrecioUnitario { get; private set; }
        
        // Propiedades calculadas
        public Dinero Subtotal => Dinero.Create(PrecioUnitario.Cantidad * Cantidad, PrecioUnitario.Moneda);
        
        // Relaciones
        public Usuario Usuario { get; private set; }
        public Articulo Articulo { get; private set; }
        
        private ItemCarrito() : base() { } 
        
        public ItemCarrito(
            Guid id,
            Guid usuarioId,
            Guid articuloId,  // Cambiado el nombre del parámetro
            int cantidad,
            Dinero precioUnitario
        ) : base(id)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(cantidad));
                
            UsuarioId = usuarioId;
            ArticuloId = articuloId;  // Cambiado
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
        }
        
        public void ActualizarCantidad(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero", nameof(cantidad));
                
            Cantidad = cantidad;
            ActualizarFechaModificacion();
        }
    }

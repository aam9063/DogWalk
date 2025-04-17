using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain.Entities;

 public class DetalleFactura : EntityBase
    {
        public Guid FacturaId { get; private set; }
        public TipoItem TipoItem { get; private set; }
        public Guid ItemId { get; private set; }  // Id del Servicio o Articulo
        public int Cantidad { get; private set; }
        public Dinero PrecioUnitario { get; private set; }
        public Dinero Subtotal { get; private set; }
        
        // Relaciones
        public Factura Factura { get; private set; }
        
        private DetalleFactura() : base() { } // Para EF Core
        
        public DetalleFactura(
            Guid id,
            Guid facturaId,
            TipoItem tipoItem,
            Guid itemId,
            int cantidad,
            Dinero precioUnitario
        ) : base(id)
        {
            FacturaId = facturaId;
            TipoItem = tipoItem;
            ItemId = itemId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = Dinero.Create(precioUnitario.Cantidad * cantidad);
        }
    }
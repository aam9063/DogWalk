using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;

namespace DogWalk_Domain.Entities;

public class DetalleFactura : EntityBase
{
    public Guid FacturaId { get; private set; }
    public Guid ArticuloId { get; private set; }  // Ahora solo manejamos artículos
    public int Cantidad { get; private set; }
    public Dinero PrecioUnitario { get; private set; }
    
    // Propiedades calculadas
    public Dinero Subtotal => Dinero.Create(PrecioUnitario.Cantidad * Cantidad, PrecioUnitario.Moneda);
    
    // Relaciones
    public Factura Factura { get; private set; }
    public Articulo Articulo { get; private set; }
    
    private DetalleFactura() : base() { } // Para EF Core
    
    public DetalleFactura(
        Guid id,
        Guid facturaId,
        Guid articuloId,
        int cantidad,
        Dinero precioUnitario
    ) : base(id)
    {
        FacturaId = facturaId;
        ArticuloId = articuloId;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }
}
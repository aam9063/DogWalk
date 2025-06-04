using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DogWalk_Domain.Entities;

 public class Factura : EntityBase
    {
        public Guid UsuarioId { get; private set; }
        public DateTime FechaFactura { get; private set; }
        public Dinero Total { get; private set; }
        public MetodoPago MetodoPago { get; private set; }
        
        // Relaciones
        public Usuario Usuario { get; private set; }
        
        private readonly List<DetalleFactura> _detalles = new();
        public IReadOnlyCollection<DetalleFactura> Detalles => _detalles.AsReadOnly();
        
        private Factura() : base() { } 
        
        public Factura(
            Guid id,
            Guid usuarioId,
            MetodoPago metodoPago
        ) : base(id)
        {
            UsuarioId = usuarioId;
            FechaFactura = DateTime.UtcNow;
            Total = Dinero.Create(0);
            MetodoPago = metodoPago;
        }
        
        public void AgregarDetalle(DetalleFactura detalle)
        {
            _detalles.Add(detalle);
            RecalcularTotal();
            ActualizarFechaModificacion();
        }
        
        private void RecalcularTotal()
        {
            var totalCalculado = _detalles.Sum(d => d.Subtotal.Cantidad);
            Total = Dinero.Create(totalCalculado);
        }
    }

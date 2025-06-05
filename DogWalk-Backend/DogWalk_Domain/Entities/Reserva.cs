using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Common.ValueObjects;
using System;

namespace DogWalk_Domain.Entities;

public class Reserva : EntityBase
    {
        public Guid UsuarioId { get; private set; }
        public Guid PaseadorId { get; private set; }
        public Guid ServicioId { get; private set; }
        public Guid PerroId { get; private set; }
        public Guid DisponibilidadId { get; private set; }
        public DateTime FechaReserva { get; private set; }
        public EstadoReserva Estado { get; private set; }
        public Dinero Precio { get; private set; }
        
        // Relaciones
        public Usuario Usuario { get; private set; }
        public Paseador Paseador { get; private set; }
        public Servicio Servicio { get; private set; }
        public Perro Perro { get; private set; }
        public DisponibilidadHoraria Disponibilidad { get; private set; }
        
        private Reserva() : base() { } 
        
        public Reserva(
            Guid id,
            Guid usuarioId,
            Guid paseadorId,
            Guid servicioId,
            Guid perroId,
            Guid disponibilidadId,
            DateTime fechaReserva,
            Dinero precio,
            EstadoReserva estado = EstadoReserva.Pendiente
        ) : base(id)
        {
            UsuarioId = usuarioId;
            PaseadorId = paseadorId;
            ServicioId = servicioId;
            PerroId = perroId;
            DisponibilidadId = disponibilidadId;
            FechaReserva = fechaReserva;
            Precio = precio;
            Estado = estado;
        }
        
        public void CambiarEstado(EstadoReserva nuevoEstado)
        {
            // Validación de cambios de estado permitidos
            if (Estado == EstadoReserva.Cancelada || Estado == EstadoReserva.Completada)
                throw new InvalidOperationException("No se puede cambiar el estado de una reserva cancelada o completada");
                
            Estado = nuevoEstado;
            ActualizarFechaModificacion();
        }
    }

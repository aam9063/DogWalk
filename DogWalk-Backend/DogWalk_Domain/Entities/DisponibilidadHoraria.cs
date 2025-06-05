using DogWalk_Domain.Common.Enums;
using System;

namespace DogWalk_Domain.Entities;

 public class DisponibilidadHoraria : EntityBase
    {
        public Guid PaseadorId { get; private set; }
        public DateTime FechaHora { get; private set; }
        public EstadoDisponibilidad Estado { get; private set; }
        
        // Relaciones
        public Paseador Paseador { get; private set; }
        
        private DisponibilidadHoraria() : base() { } 
        
        public DisponibilidadHoraria(
            Guid id,
            Guid paseadorId,
            DateTime fechaHora,
            EstadoDisponibilidad estado = EstadoDisponibilidad.Disponible
        ) : base(id)
        {
            PaseadorId = paseadorId;
            FechaHora = fechaHora;
            Estado = estado;
        }
        
        public void CambiarEstado(EstadoDisponibilidad estado)
        {
            Estado = estado;
            ActualizarFechaModificacion();
        }
    }
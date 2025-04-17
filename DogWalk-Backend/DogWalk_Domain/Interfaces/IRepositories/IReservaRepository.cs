using System;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories;

 public interface IReservaRepository : IRepository<Reserva>
    {
        Task<IEnumerable<Reserva>> GetByUsuarioIdAsync(Guid usuarioId);
        Task<IEnumerable<Reserva>> GetByPaseadorIdAsync(Guid paseadorId);
        Task<IEnumerable<Reserva>> GetByEstadoAsync(EstadoReserva estado);
        Task<IEnumerable<Reserva>> GetByFechaAsync(DateTime fecha);
    }

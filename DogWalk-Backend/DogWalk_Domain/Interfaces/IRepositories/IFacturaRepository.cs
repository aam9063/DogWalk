using System;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IFacturaRepository : IRepository<Factura>
    {
        Task<IEnumerable<Factura>> GetByUsuarioIdAsync(Guid usuarioId);
        Task<IEnumerable<Factura>> GetByFechaAsync(DateTime fechaInicio, DateTime fechaFin);
    }

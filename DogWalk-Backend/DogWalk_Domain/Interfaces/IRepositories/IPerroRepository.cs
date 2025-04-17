using System;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IPerroRepository : IRepository<Perro>
    {
        Task<IEnumerable<Perro>> GetByUsuarioIdAsync(Guid usuarioId);
    }

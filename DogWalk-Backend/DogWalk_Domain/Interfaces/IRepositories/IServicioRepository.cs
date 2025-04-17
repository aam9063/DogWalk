using System;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories;

 public interface IServicioRepository : IRepository<Servicio>
    {
        Task<IEnumerable<Servicio>> GetByTipoAsync(TipoServicio tipo);
    }

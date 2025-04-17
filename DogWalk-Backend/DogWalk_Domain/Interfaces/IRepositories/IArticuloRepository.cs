using System;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IArticuloRepository : IRepository<Articulo>
    {
        Task<IEnumerable<Articulo>> GetByCategoriaAsync(CategoriaArticulo categoria);
        Task<IEnumerable<Articulo>> GetByDisponibilidadAsync(bool disponible = true);
    }

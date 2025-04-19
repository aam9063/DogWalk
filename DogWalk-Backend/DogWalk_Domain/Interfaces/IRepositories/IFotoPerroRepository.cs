using DogWalk_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DogWalk_Domain.Interfaces.IRepositories;

public interface IFotoPerroRepository : IRepository<FotoPerro>
{
    Task<IEnumerable<FotoPerro>> GetByPerroIdAsync(Guid perroId);
}

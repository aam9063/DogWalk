using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories
{
    public interface IOpinionPerroRepository : IRepository<OpinionPerro>
    {
        Task<IEnumerable<OpinionPerro>> GetByPerroIdAsync(Guid perroId);
        Task<IEnumerable<OpinionPerro>> GetByPaseadorIdAsync(Guid paseadorId);
        Task<OpinionPerro> GetByPerroYPaseadorAsync(Guid perroId, Guid paseadorId);
        Task<double> GetPromedioPerroAsync(Guid perroId);
    }
}
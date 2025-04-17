using DogWalk_Domain.Entities;

namespace DogWalk_Domain.Interfaces.IRepositories
{
    public interface IRankingPaseadorRepository : IRepository<RankingPaseador>
    {
        Task<IEnumerable<RankingPaseador>> GetByPaseadorIdAsync(Guid paseadorId);
        Task<IEnumerable<RankingPaseador>> GetByUsuarioIdAsync(Guid usuarioId);
        Task<RankingPaseador> GetByUsuarioYPaseadorAsync(Guid usuarioId, Guid paseadorId);
        Task<double> GetPromedioPaseadorAsync(Guid paseadorId);
    }
}

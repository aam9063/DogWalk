using DogWalk_Domain.Entities;


namespace DogWalk_Domain.Interfaces.IRepositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    }
}

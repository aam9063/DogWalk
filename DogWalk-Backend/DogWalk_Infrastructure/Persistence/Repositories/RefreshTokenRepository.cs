using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;


namespace DogWalk_Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(DogWalkDbContext context) : base(context) { }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }
    }
}
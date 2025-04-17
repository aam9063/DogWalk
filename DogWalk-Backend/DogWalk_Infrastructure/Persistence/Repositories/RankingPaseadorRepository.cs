using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class RankingPaseadorRepository : GenericRepository<RankingPaseador>, IRankingPaseadorRepository
{
    public RankingPaseadorRepository(DogWalkDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<RankingPaseador>> GetByPaseadorIdAsync(Guid paseadorId)
    {
        return await _context.RankingPaseadores
            .Include(r => r.Usuario)
            .Where(r => r.PaseadorId == paseadorId)
            .OrderByDescending(r => r.CreadoEn)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<RankingPaseador>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.RankingPaseadores
            .Include(r => r.Paseador)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.CreadoEn)
            .ToListAsync();
    }
    
    public async Task<RankingPaseador> GetByUsuarioYPaseadorAsync(Guid usuarioId, Guid paseadorId)
    {
        return await _context.RankingPaseadores
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.PaseadorId == paseadorId);
    }
    
    public async Task<double> GetPromedioPaseadorAsync(Guid paseadorId)
    {
        var valoraciones = await _context.RankingPaseadores
            .Where(r => r.PaseadorId == paseadorId)
            .ToListAsync();
            
        if (!valoraciones.Any())
            return 0;
            
        return valoraciones.Average(r => r.Valoracion.Puntuacion);
    }
}

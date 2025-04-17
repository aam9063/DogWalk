using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class OpinionPerroRepository : GenericRepository<OpinionPerro>, IOpinionPerroRepository
{
    public OpinionPerroRepository(DogWalkDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<OpinionPerro>> GetByPerroIdAsync(Guid perroId)
    {
        return await _context.OpinionesPerros
            .Include(o => o.Paseador)
            .Where(o => o.PerroId == perroId)
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<OpinionPerro>> GetByPaseadorIdAsync(Guid paseadorId)
    {
        return await _context.OpinionesPerros
            .Include(o => o.Perro)
            .Where(o => o.PaseadorId == paseadorId)
            .OrderByDescending(o => o.CreadoEn)
            .ToListAsync();
    }
    
    public async Task<OpinionPerro> GetByPerroYPaseadorAsync(Guid perroId, Guid paseadorId)
    {
        return await _context.OpinionesPerros
            .FirstOrDefaultAsync(o => o.PerroId == perroId && o.PaseadorId == paseadorId);
    }
    
    public async Task<double> GetPromedioPerroAsync(Guid perroId)
    {
        var opiniones = await _context.OpinionesPerros
            .Where(o => o.PerroId == perroId)
            .ToListAsync();
            
        if (!opiniones.Any())
            return 0;
            
        return opiniones.Average(o => o.Valoracion.Puntuacion);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class ServicioRepository : GenericRepository<Servicio>, IServicioRepository
{
    public ServicioRepository(DogWalkDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Servicio>> GetByTipoAsync(TipoServicio tipo)
    {
        return await _context.Servicios
            .Where(s => s.Tipo == tipo)
            .ToListAsync();
    }

    public override async Task<Servicio> GetByIdAsync(Guid id)
    {
        return await _context.Servicios
            .Include(s => s.Precios)
                .ThenInclude(p => p.Paseador)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Servicio>> GetWithPreciosAsync()
    {
        return await _context.Servicios
            .Include(s => s.Precios)
            .Where(s => s.Precios.Any())
            .ToListAsync();
    }

    public async Task<IEnumerable<Servicio>> GetByPaseadorIdAsync(Guid paseadorId)
    {
        return await _context.Servicios
            .Include(s => s.Precios)
            .Where(s => s.Precios.Any(p => p.PaseadorId == paseadorId))
            .ToListAsync();
    }
}

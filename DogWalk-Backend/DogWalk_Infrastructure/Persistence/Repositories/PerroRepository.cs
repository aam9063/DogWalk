using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class PerroRepository : GenericRepository<Perro>, IPerroRepository
{
    public PerroRepository(DogWalkDbContext context) : base(context)
    {
    }
    
    public async Task<IEnumerable<Perro>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Perros
            .Include(p => p.Fotos)
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();
    }
    
    public override async Task<Perro> GetByIdAsync(Guid id)
    {
        return await _context.Perros
            .Include(p => p.Fotos)
            .Include(p => p.Opiniones)
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<IEnumerable<Perro>> GetWithOpinionesAsync()
    {
        return await _context.Perros
            .Include(p => p.Opiniones)
            .Where(p => p.Opiniones.Any())
            .ToListAsync();
    }
}

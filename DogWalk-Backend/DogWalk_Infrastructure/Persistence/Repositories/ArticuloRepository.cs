using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DogWalk_Infrastructure.Persistence.Repositories;

public class ArticuloRepository : GenericRepository<Articulo>, IArticuloRepository
{
    public ArticuloRepository(DogWalkDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Articulo>> GetByCategoriaAsync(CategoriaArticulo categoria)
    {
        return await _context.Articulos
            .Include(a => a.Imagenes)
            .Where(a => a.Categoria == categoria)
            .ToListAsync();
    }

    public async Task<IEnumerable<Articulo>> GetByDisponibilidadAsync(bool disponible = true)
    {
        return await _context.Articulos
            .Include(a => a.Imagenes)
            .Where(a => disponible ? a.Stock > 0 : a.Stock == 0)
            .ToListAsync();
    }

    public override async Task<Articulo> GetByIdAsync(Guid id)
    {
        return await _context.Articulos
            .Include(a => a.Imagenes)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> ReducirStockAsync(Guid id, int cantidad)
    {
        var articulo = await _dbSet.FindAsync(id);
        
        if (articulo == null || articulo.Stock < cantidad)
            return false;
            
        articulo.ActualizarStock(-cantidad); // Reducir stock
        return true;
    }

    public async Task<IEnumerable<Articulo>> BuscarAsync(string terminoBusqueda)
    {
        return await _context.Articulos
            .Include(a => a.Imagenes)
            .Where(a => a.Nombre.Contains(terminoBusqueda) || 
                        a.Descripcion.Contains(terminoBusqueda))
            .ToListAsync();
    }

    public async Task<List<Articulo>> GetArticulosByIds(List<Guid> ids)
    {
        return await _context.Articulos
            .Include(a => a.Imagenes)
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DogWalk_Domain.Common.Enums;
using DogWalk_Domain.Entities;
using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            
        articulo.ActualizarStock(-cantidad); 
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

    public virtual async Task<(IEnumerable<Articulo> Items, int TotalCount)> GetPagedAsync(
        Expression<Func<Articulo, bool>> predicate, 
        Expression<Func<Articulo, int>> orderBy, 
        bool ascending = true, 
        int pageNumber = 1, 
        int pageSize = 10)
    {
        var query = _dbSet.AsNoTracking();
        
        if (predicate != null)
            query = query.Where(predicate);
        
        // Obtener el total antes de aplicar la paginación
        var totalCount = await query.CountAsync();
        
        // Aplicar ordenamiento y paginación
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, totalCount);
    }
}

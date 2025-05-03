using DogWalk_Domain.Interfaces.IRepositories;
using DogWalk_Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DogWalk_Infrastructure.Persistence.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly DogWalkDbContext _context;
        protected readonly DbSet<T> _dbSet;
        
        public GenericRepository(DogWalkDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
        
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }
        
        public virtual Task UpdateAsync(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
        
        public virtual Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
        
        // Implementación de los métodos de paginación
        
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _dbSet.CountAsync();
            var items = await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (items, totalCount);
        }
        
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            var query = _dbSet.Where(predicate);
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (items, totalCount);
        }
        
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(
            Expression<Func<T, bool>> predicate, 
            Expression<Func<T, TKey>> orderBy, 
            bool ascending = true, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            var query = _dbSet.Where(predicate);
            var totalCount = await query.CountAsync();
            
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (items, totalCount);
        }
    }
}

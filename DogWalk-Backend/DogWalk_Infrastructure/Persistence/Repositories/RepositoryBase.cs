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
    public class RepositoryBase<T> : IRepository<T> where T : class
    {
        protected readonly DogWalkDbContext _dbContext;
        
        public RepositoryBase(DogWalkDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbContext.Set<T>().ToListAsync();
        }
        
        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            return entity;
        }
        
        public virtual async Task UpdateAsync(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }
        
        public virtual async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await Task.CompletedTask;
        }
        
        // Implementación de métodos de paginación
        
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<T>().AsNoTracking();
            var totalCount = await query.CountAsync();
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            return (items, totalCount);
        }
        
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
            Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            var query = _dbContext.Set<T>().AsNoTracking().Where(predicate);
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
            var query = _dbContext.Set<T>().AsNoTracking().Where(predicate);
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

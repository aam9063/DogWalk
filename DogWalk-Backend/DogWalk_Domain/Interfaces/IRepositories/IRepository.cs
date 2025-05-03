using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DogWalk_Domain.Interfaces.IRepositories;

 public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    
    // Métodos para paginación
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync<TKey>(
        Expression<Func<T, bool>> predicate, 
        Expression<Func<T, TKey>> orderBy, 
        bool ascending = true, 
        int pageNumber = 1, 
        int pageSize = 10);
    }

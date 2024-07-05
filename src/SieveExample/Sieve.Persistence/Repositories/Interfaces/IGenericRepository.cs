using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.Persistence.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        IGenericRepository<T> WithTracking();
        IGenericRepository<T> WithoutTracking();
        IQueryable<T> Entities { get; }
        Task<T?> GetByIdAsync(params object?[]? id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity, params object?[]? id);
        Task DeleteAsync(T entity);
        Task<bool> Exists(params object?[]? id);
    }
}

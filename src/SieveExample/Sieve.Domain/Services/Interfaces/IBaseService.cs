using Sieve.Domain.Services.Models;
using Sieve.Models;
using SieveExample.Domain.Helpers.PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sieve.Domain.Services.Interfaces
{
    public interface IBaseService<T> where T : class
    {
        IBaseService<T> WithTracking();
        IBaseService<T> WithoutTracking();
        Task<ServiceResult<T>> GetByIdAsync(params object?[]? id);
        Task<(IReadOnlyList<T> data, int totalCount)> GetPagedDataAsync(SieveModel sieveModel);
        Task<ServiceResult<IPagedList<TOut>>> SearchAsync<TOut>(SieveModel sieveModel, Func<T, TOut> formatterCallback);
        Task<ServiceResult<T>> AddAsync(T entity);
        Task<ServiceResult<T>> AddAndSaveAsync(T entity);
        Task<T> UpdateAsync(T entity, params object?[]? id);
        Task<ServiceResult<T>> UpdateAndSaveAsync(T entity, params object?[]? id);
        Task DeleteAsync(params object?[]? id);
        Task<ServiceResult<T>> DeleteAndSaveAsync(params object?[]? id);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

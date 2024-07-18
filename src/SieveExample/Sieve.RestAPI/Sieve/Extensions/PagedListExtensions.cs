using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.RestAPI.Sieve.Models;
using Sieve.Services;

namespace Sieve.RestAPI.Sieve.Extensions
{
    public static class PagedListExtensions
    {
        public static async Task<PagedList<TOut>> ToPagedListAsync<T, TOut>(this IQueryable<T> superset,
            ISieveProcessor sieveProcessor,
            IOptions<SieveOptions> sieveOptions,
            SieveModel model,
            Func<T, TOut> formatterCallback)
        {
            if (model.Page == null || model.Page < 1)
                model.Page = 1;
            if (model.PageSize == null || model.PageSize < 1)
                model.PageSize = sieveOptions.Value.DefaultPageSize;

            if (model.PageSize > sieveOptions.Value.MaxPageSize)
                model.PageSize = sieveOptions.Value.MaxPageSize;

            var filteredAndSortedData = sieveProcessor.Apply(model, superset);
            var totalItemCount = await superset.CountAsync();
            var pagedData = await filteredAndSortedData.Skip((model.Page.Value - 1) * model.PageSize.Value).Take(model.PageSize.Value).ToListAsync();

            var formattedData = pagedData.Select(formatterCallback).ToList();

            return new PagedList<TOut>(formattedData, totalItemCount, model.Page.Value, model.PageSize.Value);
        }
    }
}

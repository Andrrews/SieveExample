using Sieve.Persistence.Models;
using Sieve.Services;

namespace Sieve.RestAPI.Sieve.Services
{
    public class SieveCustomSortMethods : ISieveCustomSortMethods
    {
        public IQueryable<Student> OrderByIdAndFirstName(IQueryable<Student> source, bool useThenBy) => useThenBy
            ? ((IOrderedQueryable<Student>)source).ThenBy(p => p.Id)
            : source.OrderBy(p => p.Id)
                .ThenBy(p => p.FirstName);
    }
}

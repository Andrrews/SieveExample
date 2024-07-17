using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Persistence.Models;
using Sieve.Services;

namespace Sieve.RestAPI.Sieve.Services
{
    public class ApplicationSieveProcessor: SieveProcessor
    {
        public ApplicationSieveProcessor(IOptions<SieveOptions> options, ISieveCustomSortMethods customSortMethods, ISieveCustomFilterMethods customFilterMethods) : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            // Option 1: Map all properties centrally
            mapper.Property<Student>(p => p.Id)
                .CanSort()
                .CanFilter()
                .HasName("Id");
            mapper.Property<Student>(p => p.FirstName)
                .CanSort()
                .CanFilter()
                .HasName("FirstName");
            // Option 2: Manually apply functionally grouped mapping configurations
            //mapper.ApplyConfiguration<SieveConfigurationForPost>();
            
            // Option 3: Scan and apply all configurations
            //mapper.ApplyConfigurationsFromAssembly(typeof(ApplicationSieveProcessor).Assembly);

            return mapper;
        }
    }
}

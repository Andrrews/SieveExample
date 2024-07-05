using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sieve.Persistence.UnitOfWork;

namespace Sieve.Persistence.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
        {            
            string connectionString = (configuration.GetConnectionString("SqlLite")) ?? throw new ArgumentNullException(nameof(configuration));

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString(connectionString), b=>b.MigrationsAssembly("Sieve.Persistence"));
            });

            

            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork.UnitOfWork));
        }
    }
}

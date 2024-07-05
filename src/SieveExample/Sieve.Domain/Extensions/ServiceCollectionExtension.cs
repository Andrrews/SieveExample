using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Domain.Helpers.Sieve;
using Sieve.Domain.Services;
using Sieve.Domain.Services.Interfaces;
using Sieve.Models;
using Sieve.Services;

namespace Sieve.Domain.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddDomainLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SieveOptions>(sieveOptions =>
            {
                configuration.GetSection("Sieve").Bind(sieveOptions);
            });

            services
                .AddScoped<ISieveCustomSortMethods, SieveCustomSortMethods>()
                .AddScoped<ISieveCustomFilterMethods, SieveCustomFilterMethods>()
                .AddScoped<ISieveProcessor, SieveService>()
                .AddScoped(typeof(IStudentService), typeof(StudentService));
        }
    }
}

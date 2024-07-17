using Sieve.Domain.Extensions;
using Sieve.Domain.Utils;
using Sieve.Models;
using Sieve.Persistence;
using Sieve.Persistence.Extensions;
using Sieve.Persistence.UnitOfWork;
using Sieve.RestAPI.Sieve.Services;
using Sieve.Services;

namespace Sieve.RestAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddPersistenceLayer(builder.Configuration);
            builder.Services.AddDomainLayer(builder.Configuration);

            //minimalnie potrzebne do uruchomienia SIEVE
            builder.Services.Configure<SieveOptions>(builder.Configuration.GetSection("Sieve"));
            builder.Services.AddScoped<ISieveCustomFilterMethods, SieveCustomFilterMethods>();
            builder.Services.AddScoped<ISieveCustomSortMethods, SieveCustomSortMethods>();
            builder.Services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                using (var scope = app.Services.CreateScope())
                {
                    await DbContextSeed.SeedAsync(scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
                        scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
                }
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

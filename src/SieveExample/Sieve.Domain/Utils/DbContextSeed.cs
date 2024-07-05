using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sieve.Persistence.Models;
using Sieve.Persistence.Repositories.Interfaces;
using Sieve.Persistence.UnitOfWork;

namespace Sieve.Domain.Utils
{
    public class DbContextSeed
    {
        public static async Task SeedAsync(IUnitOfWork unitOfWork, ILoggerFactory loggerFactory)
        {
            var options = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                 JsonNumberHandling.WriteAsString
            };

            try
            {
                await PopulateTableAsync<Student>("SeedData/Students.json", unitOfWork);

            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<DbContextSeed>();
                logger.LogError(ex, "Request error");
            }
        }
        private static async Task PopulateTableAsync<T>(string jsonData, IUnitOfWork unitOfWork) where T : class
        {
            IGenericRepository<T> repository = unitOfWork.Repository<T>();

            if (repository.Entities.Any())
            {
                return;
            }

            var serializerOptions = new JsonSerializerOptions()
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString |
                                 JsonNumberHandling.WriteAsString
            };

            var entitiesData = File.ReadAllText(jsonData);

            var entities = JsonSerializer.Deserialize<List<T>>(entitiesData, serializerOptions);

            foreach (var entity in entities)
            {
                await repository.AddAsync(entity);
            }

            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sieve.Persistence.Models;
using Sieve.Persistence.Repositories.Interfaces;

namespace Sieve.Persistence.Repositories
{
    internal class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext dbContext) : base(dbContext)
        {

            
        }

        public async Task<Student?> GetByName(string name)
        {
            return Entities.FirstOrDefault(x => x.FirstName.Contains(name));
        }
    }
}

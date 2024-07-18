using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sieve.Persistence.Models;
using Sieve.Persistence.Repositories.Interfaces;

namespace Sieve.Persistence.Repositories
{
    internal class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        private AppDbContext _context;
        public StudentRepository(AppDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;

        }

        public async Task<Student?> GetByName(string name)
        {
            return Entities.FirstOrDefault(x => x.FirstName.Contains(name));
        }

        public async Task DeleteById(int id)
        {
            var result = Entities.FirstOrDefault(s => s.Id == id);
           
            if (result != null) await DeleteAsync(result);
            await _context.SaveChangesAsync();
        }
    
    }
    }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sieve.Domain.Services.Interfaces;
using Sieve.Models;
using Sieve.Persistence.Models;
using Sieve.Persistence.UnitOfWork;
using Sieve.Services;

namespace Sieve.Domain.Services
{
    internal class StudentService : BaseService<Student>, IStudentService
    {
        public StudentService(ILogger<Student> logger, ISieveProcessor? sieveService, IOptions<SieveOptions>? sieveOptions, IUnitOfWork unitOfWork) : base(logger, sieveService, sieveOptions, unitOfWork)
        {
        }
    }
}

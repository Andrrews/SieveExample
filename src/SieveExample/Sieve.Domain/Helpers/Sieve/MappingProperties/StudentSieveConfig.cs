using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sieve.Persistence.Models;
using Sieve.Services;

namespace Sieve.Domain.Helpers.Sieve.MappingProperties
{
    internal class StudentSieveConfig : ISieveConfiguration
    {
        public void Configure(SievePropertyMapper mapper)
        {
            mapper.Property<Student>(p => p.Id)
                .CanFilter()
                .CanSort();
            mapper.Property<Student>(p => p.FirstName)
                .CanFilter()
                .CanSort();
            mapper.Property<Student>(p => p.LastName)
                .CanFilter()
                .CanSort();
            mapper.Property<Student>(p => p.BirthDate)
                .CanFilter()
                .CanSort();

        }
    }

    
}

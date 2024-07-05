using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Sieve.Domain.Helpers.Sieve.MappingProperties;
using Sieve.Models;
using Sieve.Services;

namespace Sieve.Domain.Services
{
    public class SieveService : SieveProcessor 
    {
        public SieveService(
            IOptions<SieveOptions> options,
            ISieveCustomSortMethods customSortMethods,
            ISieveCustomFilterMethods customFilterMethods)
            : base(options, customSortMethods, customFilterMethods)
        {
        }

        protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
        {
            return mapper
                .ApplyConfiguration<StudentSieveConfig>();
    
        }
    }
}

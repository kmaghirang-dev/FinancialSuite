using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace FinancialSuite.Infrastructure.Authorization.Requirements.MinimumAgeRequirement
{
    public class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
    {
        public int MinimumAge { get; set; } = minimumAge;   
    }
}

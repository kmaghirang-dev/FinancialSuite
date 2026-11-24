using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Infrastructure.Authorization
{
    public static class PolicyNames
    {
        public const string HasNationality = "HasNationality";
        public const string AtLeast20 = "AtLeast20";
    }

    public static class AppClaimTypes
    {
        public const string Nationality = "Nationality";
        // Must match the claim type UserContext reads
        public const string DateOfBirth = "DateOfBirth";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.Users
{
    public record CurrentUser(string Id, string UserName, IEnumerable<string> Roles, DateOnly? DateOfBirth)
    {
        public bool IsInRole(string role) => Roles.Contains(role);
    }
}

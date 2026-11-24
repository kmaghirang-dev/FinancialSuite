using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.Users.Commands.UnassignUserRole
{
    public class UnassignUserRoleCommand : IRequest
    {
        public string UserEmail { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

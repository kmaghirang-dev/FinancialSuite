using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users.Commands.AssignUserRole;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.Users.Commands.UnassignUserRole
{
    public class UnassignUserRoleCommandHandler(
        ILogger<AssignUserRoleCommandHandler> logger,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager) : IRequestHandler<UnassignUserRoleCommand>  
    {
        public async Task Handle(UnassignUserRoleCommand request, CancellationToken cancellationToken)
        {
            var userEmail = request.UserEmail;

            var user = await userManager.FindByEmailAsync(userEmail)
                       ?? throw new NotFoundException(nameof(User), userEmail);

            var roleName = request.Role;

            var role = await roleManager.FindByNameAsync(roleName)
                       ?? throw new NotFoundException(nameof(IdentityRole), roleName);

            await userManager.RemoveFromRoleAsync(user, role.Name!);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;

namespace FinancialSuite.Application.Users.Commands.AssignUserRole
{
    public class AssignUserRoleCommandHandler(
        ILogger<AssignUserRoleCommandHandler> logger, 
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager) : IRequestHandler<AssignUserRoleCommand>
    {
        public async Task Handle(AssignUserRoleCommand request, CancellationToken cancellationToken)
        {
            var userEmail = request.UserEmail;

            var user = await userManager.FindByEmailAsync(userEmail)
                ?? throw new NotFoundException(nameof(User), userEmail);

            var roleName = request.Role;

            var role = await roleManager.FindByNameAsync(roleName)
                ?? throw new NotFoundException(nameof(IdentityRole), roleName);

            await userManager.AddToRoleAsync(user, role.Name!);
        }
    }
}

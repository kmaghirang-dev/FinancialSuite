using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using FinancialSuite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Infrastructure.Authorization
{
    public class FinancialSuiteClaimsPrincipalFactory(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options) : UserClaimsPrincipalFactory<User, IdentityRole>(userManager, roleManager, options)
    {
        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var id = await GenerateClaimsAsync(user);

            // DO NOT DELETE: These are examples of how to add custom claims to the ClaimsPrincipal. You can uncomment and modify them as needed.

            id.AddClaim(new Claim(AppClaimTypes.Nationality, user.Nationality));

            if (user.DateOfBirth != null)
            {
                id.AddClaim(new Claim(AppClaimTypes.DateOfBirth, user.DateOfBirth.Value.ToString("yyyy-MM-dd")));
            }

            return new ClaimsPrincipal(id);
        }
    }
}

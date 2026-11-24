using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;

namespace FinancialSuite.Infrastructure.Authorization.Requirements.MinimumAgeRequirement
{
    internal class MinimumAgeRequirementHandler(
        ILogger<MinimumAgeRequirementHandler> logger,
        IUserContext userContext) : AuthorizationHandler<MinimumAgeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinimumAgeRequirement requirement)
        {
            var currentUser = userContext.GetCurrentUser();

            var dob = currentUser?.DateOfBirth;

            if (dob == null)
            {
                logger.LogWarning("User date of birth is missing; failing minimum age requirement");
                context.Fail();
                return Task.CompletedTask;
            }

            if (dob.Value.AddYears(requirement.MinimumAge) <= DateOnly.FromDateTime(DateTime.Today))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}

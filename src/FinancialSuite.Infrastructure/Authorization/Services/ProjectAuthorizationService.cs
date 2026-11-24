using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Interfaces;

namespace FinancialSuite.Infrastructure.Authorization.Services
{
    public class ProjectAuthorizationService(
        ILogger<ProjectAuthorizationService> logger,
        IUserContext userContext) : IProjectAuthorizationService
    {
        public bool Authorize(Project project, ResourceOperation resourceOperation)
        {
            var currentUser = userContext.GetCurrentUser();

            if (currentUser == null) return false;

            //logger.LogInformation("Authorizing user {UserId} to {ResourceOperation} for project {ProjectId}", currentUser!.Id, resourceOperation, project.ProjectId);

            if (resourceOperation ==  ResourceOperation.Read || resourceOperation == ResourceOperation.Create)
            {
                logger.LogInformation("Create/Read operation - successful authorization");
                return true;
            }

            if (resourceOperation ==  ResourceOperation.Delete && currentUser!.IsInRole(UserRoles.SuperAdmin))
            {
                logger.LogInformation("Delete operation - successful authorization");
                return true;
            }

            if ((resourceOperation == ResourceOperation.Delete || resourceOperation == ResourceOperation.Update) &&  currentUser!.Id == project.OwnerId)
            {
                logger.LogInformation("Project Owner - successful authorization");
                return true;
            }

            return false;
        }
    }
}

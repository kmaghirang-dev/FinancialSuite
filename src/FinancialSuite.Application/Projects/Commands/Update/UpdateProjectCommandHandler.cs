using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Interfaces;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Projects.Commands.Update
{
    public class UpdateProjectCommandHandler(
        ILogger<UpdateProjectCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IProjectsRepository repository,
        IProjectAuthorizationService projectAuthorizationService) : IRequestHandler<UpdateProjectCommand>
    {
        public async Task Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var id = request.ProjectId;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Projects), id.ToString());

            if (!projectAuthorizationService.Authorize(data, ResourceOperation.Update)) throw new ForbidException();

            mapper.Map(request, data);
            data.UpdatedBy = currentUser.UserName;

            await repository.SaveChanges();
        }
    }
}

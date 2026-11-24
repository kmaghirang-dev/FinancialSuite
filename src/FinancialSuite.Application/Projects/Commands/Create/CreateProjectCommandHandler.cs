using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Projects.Commands.Create
{
    public class CreateProjectCommandHandler(
        ILogger<CreateProjectCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IProjectsRepository repository) : IRequestHandler<CreateProjectCommand, Guid>
    {
        public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var project = mapper.Map<Domain.Entities.Project>(request);
            project.OwnerId = currentUser.Id;
            project.CreatedBy = currentUser.UserName;
            project.UpdatedBy = currentUser.UserName;

            var id = await repository.CreateAsync(project);
            return id;
        }
    }
}

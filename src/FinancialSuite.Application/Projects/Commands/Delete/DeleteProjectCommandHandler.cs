using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Interfaces;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Projects.Commands.Delete
{
    public class DeleteProjectCommandHandler(
        ILogger<DeleteProjectCommandHandler> logger, 
        IProjectsRepository repository,
        IProjectAuthorizationService projectAuthorizationService) : IRequestHandler<DeleteProjectCommand>
    {
        public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Projects), id.ToString());

            if (!projectAuthorizationService.Authorize(data, ResourceOperation.Delete)) throw new ForbidException();

            await repository.DeleteAsync(data);
        }
    }
}

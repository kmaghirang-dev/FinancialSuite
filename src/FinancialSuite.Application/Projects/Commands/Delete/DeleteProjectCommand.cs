using MediatR;

namespace FinancialSuite.Application.Projects.Commands.Delete
{
    public class DeleteProjectCommand(Guid id) : IRequest
    {
        public Guid Id { get; } = id;
    }
}

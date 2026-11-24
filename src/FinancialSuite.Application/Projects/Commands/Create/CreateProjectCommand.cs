using MediatR;

namespace FinancialSuite.Application.Projects.Commands.Create
{
    public class CreateProjectCommand : IRequest<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

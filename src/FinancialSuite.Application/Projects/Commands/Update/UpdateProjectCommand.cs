using MediatR;

namespace FinancialSuite.Application.Projects.Commands.Update
{
    public class UpdateProjectCommand : IRequest
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

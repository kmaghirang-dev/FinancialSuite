using MediatR;

namespace FinancialSuite.Application.Projects.Commands.Toggle
{
    public class ToggleProjectCommand : IRequest
    {
        public Guid ProjectId { get; set; }
        public bool IsActive { get; set; }
    }
}

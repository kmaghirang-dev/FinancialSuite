using MediatR;

namespace FinancialSuite.Application.MediaLibrary.Commands.Toggle
{
    public class ToggleMediaLibraryCommand : IRequest
    {
        public Guid MediaId { get; set; }
        public bool IsActive { get; set; }
    }
}

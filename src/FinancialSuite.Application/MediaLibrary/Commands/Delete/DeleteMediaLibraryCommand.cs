using MediatR;

namespace FinancialSuite.Application.MediaLibrary.Commands.Delete
{
    public class DeleteMediaLibraryCommand(Guid id) : IRequest
    {
        public Guid Id { get; } = id;
    }
}

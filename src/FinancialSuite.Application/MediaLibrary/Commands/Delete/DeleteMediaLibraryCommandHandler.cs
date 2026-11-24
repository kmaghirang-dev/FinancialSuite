using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Commands.Delete
{
    public class DeleteMediaLibraryCommandHandler(
        ILogger<DeleteMediaLibraryCommandHandler> logger,
        IMediaLibraryRepository repository) : IRequestHandler<DeleteMediaLibraryCommand>
    {
        public async Task Handle(DeleteMediaLibraryCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(MediaLibrary), id.ToString());

            await repository.DeleteAsync(data);
        }
    }
}

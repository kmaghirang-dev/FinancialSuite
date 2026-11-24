using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Commands.Toggle
{
    public class ToggleMediaLibraryCommandHandler(
        ILogger<ToggleMediaLibraryCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IMediaLibraryRepository repository) : IRequestHandler<ToggleMediaLibraryCommand>
    {
        public async Task Handle(ToggleMediaLibraryCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var id = request.MediaId;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(MediaLibrary), id.ToString());

            mapper.Map(request, data);
            data.UpdatedBy = currentUser.UserName;

            await repository.SaveChanges();
        }
    }
}

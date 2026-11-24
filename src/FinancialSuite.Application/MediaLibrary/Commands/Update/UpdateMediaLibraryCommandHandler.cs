using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Commands.Update
{
    public class UpdateMediaLibraryCommandHandler(
        ILogger<UpdateMediaLibraryCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IMediaLibraryRepository repository) : IRequestHandler<UpdateMediaLibraryCommand>
    {
        public async Task Handle(UpdateMediaLibraryCommand request, CancellationToken cancellationToken)
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

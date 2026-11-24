using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Commands.Create
{
    public class CreateMediaLibraryCommandHandler(
        ILogger<CreateMediaLibraryCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        IMediaLibraryRepository repository) : IRequestHandler<CreateMediaLibraryCommand, Guid>
    {
        public async Task<Guid> Handle(CreateMediaLibraryCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var data = mapper.Map<Domain.Entities.MediaLibrary>(request);
            data.CreatedBy = currentUser.UserName;
            data.UpdatedBy = currentUser.UserName;

            var id = await repository.CreateAsync(data);
            return id;
        }
    }
}

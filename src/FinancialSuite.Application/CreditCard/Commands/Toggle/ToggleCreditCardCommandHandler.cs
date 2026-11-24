using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Commands.Toggle
{
    public class ToggleCreditCardCommandHandler(
        ILogger<ToggleCreditCardCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardRepository repository) : IRequestHandler<ToggleCreditCardCommand>
    {
        public async Task Handle(ToggleCreditCardCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var id = request.CreditCardId;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), id.ToString());

            mapper.Map(request, data);
            data.UpdatedBy = currentUser.UserName;

            await repository.SaveChanges();
        }
    }
}

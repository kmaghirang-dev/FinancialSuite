using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Commands.Create
{
    public class CreateCreditCardCommandHandler(
        ILogger<CreateCreditCardCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardRepository repository) : IRequestHandler<CreateCreditCardCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCreditCardCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var data = mapper.Map<Domain.Entities.CreditCard>(request);
            data.CreatedBy = currentUser.UserName;
            data.UpdatedBy = currentUser.UserName;

            return await repository.CreateAsync(data);
        }
    }
}

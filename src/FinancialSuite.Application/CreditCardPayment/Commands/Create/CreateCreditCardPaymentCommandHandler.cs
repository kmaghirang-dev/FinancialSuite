using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Create
{
    public class CreateCreditCardPaymentCommandHandler(
        ILogger<CreateCreditCardPaymentCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardPaymentRepository repository,
        ICreditCardRepository cardRepository) : IRequestHandler<CreateCreditCardPaymentCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCreditCardPaymentCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            // The card must exist; a payment on or before its initial balance date is kept but not counted (already in that balance).
            _ = await cardRepository.GetByIdAsync(request.CreditCardId) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), request.CreditCardId.ToString());

            var data = mapper.Map<Domain.Entities.CreditCardPayment>(request);
            data.CreatedBy = currentUser.UserName;
            data.UpdatedBy = currentUser.UserName;

            return await repository.CreateAsync(data);
        }
    }
}

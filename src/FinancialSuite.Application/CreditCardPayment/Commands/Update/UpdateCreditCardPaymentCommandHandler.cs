using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Update
{
    // Period totals are derived from payment dates whenever they are read, so an edit needs no other update.
    public class UpdateCreditCardPaymentCommandHandler(
        ILogger<UpdateCreditCardPaymentCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardPaymentRepository repository,
        ICreditCardRepository cardRepository) : IRequestHandler<UpdateCreditCardPaymentCommand>
    {
        public async Task Handle(UpdateCreditCardPaymentCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var id = request.CreditCardPaymentId;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCardPayment), id.ToString());

            var card = await cardRepository.GetByIdAsync(request.CreditCardId) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), request.CreditCardId.ToString());

            mapper.Map(request, data);
            data.CreditCard = card;
            data.UpdatedBy = currentUser.UserName;

            await repository.SaveChanges();
        }
    }
}

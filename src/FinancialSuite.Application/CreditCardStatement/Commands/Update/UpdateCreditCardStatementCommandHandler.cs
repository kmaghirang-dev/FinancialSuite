using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Update
{
    public class UpdateCreditCardStatementCommandHandler(
        ILogger<UpdateCreditCardStatementCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardStatementRepository repository,
        ICreditCardRepository cardRepository) : IRequestHandler<UpdateCreditCardStatementCommand>
    {
        public async Task Handle(UpdateCreditCardStatementCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var id = request.CreditCardStatementId;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCardStatement), id.ToString());

            if (data.CreditCardId != request.CreditCardId)
                throw new ConflictException("A statement cannot be moved to another card. Delete it and record it on the other card instead.");

            var card = await cardRepository.GetWithHistoryAsync(data.CreditCardId) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), data.CreditCardId.ToString());

            CreditCardStatementRules.EnsureFits(card, request.PeriodStartDate, request.PeriodEndDate, id);

            mapper.Map(request, data);
            data.UpdatedBy = currentUser.UserName;

            // A changed balance or cutoff changes the opening balance of every later statement; payments are
            // re-attributed to periods when the overview is calculated.
            CreditCardDebtCalculator.SyncOpeningBalances(card.InitialBalance, card.Statements);

            await repository.SaveChanges();
        }
    }
}

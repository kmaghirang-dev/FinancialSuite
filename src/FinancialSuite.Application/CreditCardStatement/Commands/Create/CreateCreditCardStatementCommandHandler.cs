using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Create
{
    public class CreateCreditCardStatementCommandHandler(
        ILogger<CreateCreditCardStatementCommandHandler> logger,
        IMapper mapper,
        IUserContext userContext,
        ICreditCardRepository cardRepository) : IRequestHandler<CreateCreditCardStatementCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCreditCardStatementCommand request, CancellationToken cancellationToken)
        {
            var currentUser = userContext.GetCurrentUser()!;

            var card = await cardRepository.GetWithHistoryAsync(request.CreditCardId) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), request.CreditCardId.ToString());

            CreditCardStatementRules.EnsureFits(card, request.PeriodStartDate, request.PeriodEndDate, null);

            // The key is left empty so EF treats the statement as new when it finds it in card.Statements.
            var data = mapper.Map<Domain.Entities.CreditCardStatement>(request);
            data.CreatedBy = currentUser.UserName;
            data.UpdatedBy = currentUser.UserName;
            card.Statements.Add(data);

            // Sets this statement's opening balance, and the next one's when it is inserted before a later statement.
            CreditCardDebtCalculator.SyncOpeningBalances(card.InitialBalance, card.Statements);

            await cardRepository.SaveChanges();
            return data.CreditCardStatementId;
        }
    }
}

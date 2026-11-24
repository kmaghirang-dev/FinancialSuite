using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Delete
{
    public class DeleteCreditCardStatementCommandHandler(
        ILogger<DeleteCreditCardStatementCommandHandler> logger,
        ICreditCardStatementRepository repository,
        ICreditCardRepository cardRepository) : IRequestHandler<DeleteCreditCardStatementCommand>
    {
        public async Task Handle(DeleteCreditCardStatementCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCardStatement), id.ToString());

            var card = await cardRepository.GetWithHistoryAsync(data.CreditCardId) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), data.CreditCardId.ToString());

            // The statement after it now opens from the statement before it; both changes are saved together.
            CreditCardDebtCalculator.SyncOpeningBalances(card.InitialBalance, card.Statements.Where(x => x.CreditCardStatementId != id));

            await repository.DeleteAsync(data);
        }
    }
}

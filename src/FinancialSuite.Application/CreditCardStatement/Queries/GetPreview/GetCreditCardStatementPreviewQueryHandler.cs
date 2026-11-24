using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.CreditCardStatement.Commands;
using FinancialSuite.Application.CreditCardStatement.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardStatement.Queries.GetPreview
{
    public class GetCreditCardStatementPreviewQueryHandler(ILogger<GetCreditCardStatementPreviewQueryHandler> logger, ICreditCardRepository cardRepository) : IRequestHandler<GetCreditCardStatementPreviewQuery, CreditCardStatementPreviewDto>
    {
        public async Task<CreditCardStatementPreviewDto> Handle(GetCreditCardStatementPreviewQuery request, CancellationToken cancellationToken)
        {
            var card = await cardRepository.GetWithHistoryAsync(request.CreditCardId) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), request.CreditCardId.ToString());

            return Preview(card, request);
        }

        // Runs the same calculation the saved statement will get, with the candidate statement in place.
        public static CreditCardStatementPreviewDto Preview(Domain.Entities.CreditCard card, GetCreditCardStatementPreviewQuery request)
        {
            var candidateId = request.CreditCardStatementId ?? Guid.Empty;
            var others = card.Statements.Where(x => x.CreditCardStatementId != candidateId).ToList();
            var start = request.PeriodStartDate ?? request.PeriodEndDate;

            string? problem = null;
            try
            {
                CreditCardStatementRules.EnsureFits(card, start, request.PeriodEndDate, candidateId);
            }
            catch (ConflictException conflict)
            {
                problem = conflict.Message;
            }

            var statements = others
                .Select(CreditCardDebtCalculator.ToLedger)
                .Append(new LedgerStatement(candidateId, request.PeriodEndDate, start, request.PeriodEndDate, request.ClosingBalance ?? 0));

            var ledger = CreditCardDebtCalculator.Calculate(card.InitialBalance, card.InitialBalanceDate, statements, card.Payments.Select(CreditCardDebtCalculator.ToLedger));
            var index = ledger.Periods.ToList().FindIndex(x => x.StatementId == candidateId);
            var period = ledger.Periods[index];
            var hasBalance = request.ClosingBalance != null;

            return new CreditCardStatementPreviewDto
            {
                PreviousBalance = period.OpeningBalance,
                PreviousCutoffDate = period.PreviousCutoffDate,
                OpensFromInitialBalance = period.OpensFromInitialBalance,
                PreviousStatementDate = index > 0 ? ledger.Periods[index - 1].StatementDate : null,
                PaymentsSincePrevious = period.TotalPayments,
                PaymentCount = period.PaymentCount,
                NewBalance = request.ClosingBalance,
                EstimatedNetActivity = hasBalance ? period.EstimatedNetActivity : null,
                NetDebtMovement = hasBalance ? period.NetDebtMovement : null,
                DebtStatus = hasBalance ? period.DebtStatus : null,
                LaterStatementCount = ledger.Periods.Count - index - 1,
                Problem = problem
            };
        }
    }
}

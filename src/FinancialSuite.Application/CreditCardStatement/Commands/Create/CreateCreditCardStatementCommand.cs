using MediatR;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Create
{
    // The opening balance is not sent: it is always the previous statement's closing balance.
    public class CreateCreditCardStatementCommand : IRequest<Guid>, ICreditCardStatementFields
    {
        public Guid CreditCardId { get; set; }
        public DateOnly StatementDate { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public decimal ClosingBalance { get; set; }
        public string? Notes { get; set; }
    }
}

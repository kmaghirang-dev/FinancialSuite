using MediatR;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Update
{
    public class UpdateCreditCardStatementCommand : IRequest, ICreditCardStatementFields
    {
        public Guid CreditCardStatementId { get; set; }
        // A statement stays on its card; this must match the stored card.
        public Guid CreditCardId { get; set; }
        public DateOnly StatementDate { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public decimal ClosingBalance { get; set; }
        public string? Notes { get; set; }
    }
}

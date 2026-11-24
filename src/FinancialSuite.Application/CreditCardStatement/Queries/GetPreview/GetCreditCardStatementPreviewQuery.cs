using MediatR;
using FinancialSuite.Application.CreditCardStatement.Dtos;

namespace FinancialSuite.Application.CreditCardStatement.Queries.GetPreview
{
    public class GetCreditCardStatementPreviewQuery : IRequest<CreditCardStatementPreviewDto>
    {
        public Guid CreditCardId { get; set; }
        // Defaults to the day after the latest cutoff when omitted.
        public DateOnly? PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public decimal? ClosingBalance { get; set; }
        // Set when previewing an edit, so the statement is compared without its stored version.
        public Guid? CreditCardStatementId { get; set; }
    }
}

using MediatR;

namespace FinancialSuite.Application.CreditCard.Commands.Create
{
    public class CreateCreditCardCommand : IRequest<Guid>, ICreditCardFields
    {
        public string Name { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public decimal? CreditLimit { get; set; }
        // Outstanding balance on the latest available statement; the baseline the next statement is compared against.
        public decimal InitialBalance { get; set; }
        // Cutoff (period end) of that statement.
        public DateOnly InitialBalanceDate { get; set; }
        public int? StatementCutoffDay { get; set; }
        public int? PaymentDueDay { get; set; }
        public string? Notes { get; set; }
    }
}

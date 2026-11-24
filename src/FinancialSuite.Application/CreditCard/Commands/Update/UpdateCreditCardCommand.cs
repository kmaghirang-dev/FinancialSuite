using MediatR;

namespace FinancialSuite.Application.CreditCard.Commands.Update
{
    public class UpdateCreditCardCommand : IRequest, ICreditCardFields
    {
        public Guid CreditCardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal InitialBalance { get; set; }
        public DateOnly InitialBalanceDate { get; set; }
        public int? StatementCutoffDay { get; set; }
        public int? PaymentDueDay { get; set; }
        public string? Notes { get; set; }
    }
}

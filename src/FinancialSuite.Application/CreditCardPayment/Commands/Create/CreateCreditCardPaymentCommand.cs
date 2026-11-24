using MediatR;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Create
{
    public class CreateCreditCardPaymentCommand : IRequest<Guid>, ICreditCardPaymentFields
    {
        public Guid CreditCardId { get; set; }
        public DateOnly PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentReference { get; set; }
        public string? Notes { get; set; }
    }
}

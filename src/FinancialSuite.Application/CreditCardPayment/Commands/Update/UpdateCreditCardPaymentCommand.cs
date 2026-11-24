using MediatR;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Update
{
    public class UpdateCreditCardPaymentCommand : IRequest, ICreditCardPaymentFields
    {
        public Guid CreditCardPaymentId { get; set; }
        // May differ from the stored card, to fix a payment recorded on the wrong card.
        public Guid CreditCardId { get; set; }
        public DateOnly PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentReference { get; set; }
        public string? Notes { get; set; }
    }
}

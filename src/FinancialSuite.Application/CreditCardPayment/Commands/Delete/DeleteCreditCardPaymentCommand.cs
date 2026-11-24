using MediatR;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Delete
{
    public class DeleteCreditCardPaymentCommand(Guid id) : IRequest
    {
        public Guid Id { get; } = id;
    }
}

using FluentValidation;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Create
{
    public class CreateCreditCardPaymentCommandValidator : AbstractValidator<CreateCreditCardPaymentCommand>
    {
        public CreateCreditCardPaymentCommandValidator()
        {
            Include(new CreditCardPaymentFieldsValidator());
        }
    }
}

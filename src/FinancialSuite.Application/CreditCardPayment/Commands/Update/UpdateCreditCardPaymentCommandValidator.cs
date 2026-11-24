using FluentValidation;

namespace FinancialSuite.Application.CreditCardPayment.Commands.Update
{
    public class UpdateCreditCardPaymentCommandValidator : AbstractValidator<UpdateCreditCardPaymentCommand>
    {
        public UpdateCreditCardPaymentCommandValidator()
        {
            Include(new CreditCardPaymentFieldsValidator());
        }
    }
}

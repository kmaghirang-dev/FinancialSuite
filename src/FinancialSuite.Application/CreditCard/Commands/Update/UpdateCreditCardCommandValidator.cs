using FluentValidation;

namespace FinancialSuite.Application.CreditCard.Commands.Update
{
    public class UpdateCreditCardCommandValidator : AbstractValidator<UpdateCreditCardCommand>
    {
        public UpdateCreditCardCommandValidator()
        {
            Include(new CreditCardFieldsValidator());
        }
    }
}

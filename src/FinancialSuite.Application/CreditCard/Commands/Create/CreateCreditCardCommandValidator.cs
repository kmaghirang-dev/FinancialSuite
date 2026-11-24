using FluentValidation;

namespace FinancialSuite.Application.CreditCard.Commands.Create
{
    public class CreateCreditCardCommandValidator : AbstractValidator<CreateCreditCardCommand>
    {
        public CreateCreditCardCommandValidator()
        {
            Include(new CreditCardFieldsValidator());
        }
    }
}

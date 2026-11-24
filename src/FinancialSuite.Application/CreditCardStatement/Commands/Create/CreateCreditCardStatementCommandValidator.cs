using FluentValidation;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Create
{
    public class CreateCreditCardStatementCommandValidator : AbstractValidator<CreateCreditCardStatementCommand>
    {
        public CreateCreditCardStatementCommandValidator()
        {
            Include(new CreditCardStatementFieldsValidator());
        }
    }
}

using FluentValidation;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Update
{
    public class UpdateCreditCardStatementCommandValidator : AbstractValidator<UpdateCreditCardStatementCommand>
    {
        public UpdateCreditCardStatementCommandValidator()
        {
            Include(new CreditCardStatementFieldsValidator());
        }
    }
}

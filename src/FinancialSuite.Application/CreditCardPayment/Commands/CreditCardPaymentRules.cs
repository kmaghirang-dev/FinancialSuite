using FluentValidation;
using FinancialSuite.Application.CreditCard.Commands;

namespace FinancialSuite.Application.CreditCardPayment.Commands
{
    // The editable payment fields, shared by the create and update commands.
    public interface ICreditCardPaymentFields
    {
        Guid CreditCardId { get; }
        DateOnly PaymentDate { get; }
        decimal Amount { get; }
        string? PaymentReference { get; }
        string? Notes { get; }
    }

    public class CreditCardPaymentFieldsValidator : AbstractValidator<ICreditCardPaymentFields>
    {
        public CreditCardPaymentFieldsValidator()
        {
            RuleFor(x => x.CreditCardId).NotEmpty();

            RuleFor(x => x.PaymentDate)
                .NotEmpty()
                .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("The payment date cannot be in the future.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .LessThanOrEqualTo(CreditCardRules.MaxAmount);

            RuleFor(x => x.PaymentReference).MaximumLength(100);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}

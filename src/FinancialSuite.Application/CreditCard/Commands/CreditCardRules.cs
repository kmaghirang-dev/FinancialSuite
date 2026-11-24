using FluentValidation;

namespace FinancialSuite.Application.CreditCard.Commands
{
    // The editable card fields, shared by the create and update commands.
    public interface ICreditCardFields
    {
        string Name { get; }
        string? Nickname { get; }
        decimal? CreditLimit { get; }
        decimal InitialBalance { get; }
        DateOnly InitialBalanceDate { get; }
        int? StatementCutoffDay { get; }
        int? PaymentDueDay { get; }
        string? Notes { get; }
    }

    public static class CreditCardRules
    {
        // Largest amount accepted anywhere in the tracker; well inside decimal(18,2).
        public const decimal MaxAmount = 999_999_999_999.99m;
    }

    public class CreditCardFieldsValidator : AbstractValidator<ICreditCardFields>
    {
        public CreditCardFieldsValidator()
        {
            RuleFor(x => x.Name)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("'Name' must not be empty.")
                .MaximumLength(100);

            RuleFor(x => x.Nickname).MaximumLength(100);

            RuleFor(x => x.CreditLimit)
                .GreaterThan(0)
                .LessThanOrEqualTo(CreditCardRules.MaxAmount)
                .When(x => x.CreditLimit != null);

            RuleFor(x => x.InitialBalance)
                .InclusiveBetween(-CreditCardRules.MaxAmount, CreditCardRules.MaxAmount);

            RuleFor(x => x.InitialBalanceDate)
                .NotEmpty()
                .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Initial balance date cannot be in the future.");

            RuleFor(x => x.StatementCutoffDay).InclusiveBetween(1, 31).When(x => x.StatementCutoffDay != null);
            RuleFor(x => x.PaymentDueDay).InclusiveBetween(1, 31).When(x => x.PaymentDueDay != null);
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}

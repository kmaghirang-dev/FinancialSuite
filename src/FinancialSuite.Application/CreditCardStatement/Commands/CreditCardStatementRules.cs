using FluentValidation;
using FinancialSuite.Application.CreditCard.Commands;
using FinancialSuite.Domain.Exceptions;

namespace FinancialSuite.Application.CreditCardStatement.Commands
{
    // The editable statement fields, shared by the create and update commands.
    public interface ICreditCardStatementFields
    {
        Guid CreditCardId { get; }
        DateOnly StatementDate { get; }
        DateOnly PeriodStartDate { get; }
        DateOnly PeriodEndDate { get; }
        decimal ClosingBalance { get; }
        string? Notes { get; }
    }

    public class CreditCardStatementFieldsValidator : AbstractValidator<ICreditCardStatementFields>
    {
        public CreditCardStatementFieldsValidator()
        {
            RuleFor(x => x.CreditCardId).NotEmpty();
            RuleFor(x => x.StatementDate).NotEmpty();
            RuleFor(x => x.PeriodStartDate).NotEmpty();
            RuleFor(x => x.PeriodEndDate).NotEmpty();

            RuleFor(x => x.PeriodEndDate)
                .Must((statement, end) => end >= statement.PeriodStartDate)
                .WithMessage("The period end date must be on or after the period start date.");

            RuleFor(x => x.StatementDate)
                .Must((statement, date) => date >= statement.PeriodEndDate)
                .WithMessage("The statement date must be on or after the period end date.")
                .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("The statement date cannot be in the future.");

            // Negative = a credit balance (overpaid).
            RuleFor(x => x.ClosingBalance)
                .InclusiveBetween(-CreditCardRules.MaxAmount, CreditCardRules.MaxAmount);

            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }

    public static class CreditCardStatementRules
    {
        // Rules that need the card's other statements. excludeStatementId is the statement being edited.
        public static void EnsureFits(Domain.Entities.CreditCard card, DateOnly periodStartDate, DateOnly periodEndDate, Guid? excludeStatementId)
        {
            if (periodEndDate <= card.InitialBalanceDate)
                throw new ConflictException($"The statement period must end after the card's initial balance date ({card.InitialBalanceDate:MMM d, yyyy}).");

            var overlapping = card.Statements
                .Where(x => x.CreditCardStatementId != excludeStatementId)
                .OrderBy(x => x.PeriodEndDate)
                .FirstOrDefault(x => periodStartDate <= x.PeriodEndDate && x.PeriodStartDate <= periodEndDate);

            if (overlapping != null)
                throw new ConflictException($"This period overlaps the statement for {overlapping.PeriodStartDate:MMM d, yyyy} – {overlapping.PeriodEndDate:MMM d, yyyy}.");
        }
    }
}

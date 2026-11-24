using FluentValidation;

namespace FinancialSuite.Application.CreditCard.Queries.GetOverview
{
    public class GetCreditCardOverviewQueryValidator : AbstractValidator<GetCreditCardOverviewQuery>
    {
        public GetCreditCardOverviewQueryValidator()
        {
            RuleFor(x => x.To)
                .Must((query, to) => query.From == null || to == null || to >= query.From)
                .WithMessage("'To' must be on or after 'From'.");
        }
    }
}

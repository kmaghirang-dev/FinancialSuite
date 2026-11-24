using FluentValidation;
using FinancialSuite.Application.Common;

namespace FinancialSuite.Application.CreditCard.Queries.GetAllPaginated
{
    public class GetAllPaginatedCreditCardQueryValidator : AbstractValidator<GetAllPaginatedCreditCardQuery>
    {
        private readonly int[] allowedPageSizes = [5, 10, 15, 30];
        private readonly string[] sortBy = [nameof(Domain.Entities.CreditCard.Name), nameof(Domain.Entities.CreditCard.InitialBalanceDate), nameof(Domain.Entities.CreditCard.DateCreated)];
        private readonly SortDirection[] allowedSortDirections = [SortDirection.Ascending, SortDirection.Descending];

        public GetAllPaginatedCreditCardQueryValidator()
        {
            RuleFor(r => r.PageNumber)
                .GreaterThanOrEqualTo(1);

            RuleFor(r => r.PageSize)
                .Must(value => allowedPageSizes.Contains(value))
                .WithMessage($"Page size must be one of the allowed values. ({string.Join(", ", allowedPageSizes)})");

            RuleFor(r => r.SortBy)
                .Must(value => sortBy.Contains(value))
                .When(q => q.SortBy != null)
                .WithMessage($"Sort by must be one of the allowed values. ({string.Join(", ", sortBy)})");

            RuleFor(r => r.SortDirection)
                .Must(value => allowedSortDirections.Contains(value))
                .When(q => q.SortBy != null)
                .WithMessage($"Sort direction must be one of the allowed values. ({string.Join(", ", allowedSortDirections)})");
        }
    }
}

using MediatR;
using FinancialSuite.Application.CreditCard.Dtos;

namespace FinancialSuite.Application.CreditCard.Queries.GetOverview
{
    public class GetCreditCardOverviewQuery : IRequest<CreditCardOverviewDto>
    {
        // One card (active or archived); null combines every active card.
        public Guid? CreditCardId { get; set; }
        // Inclusive range; either end may be left open.
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
    }
}

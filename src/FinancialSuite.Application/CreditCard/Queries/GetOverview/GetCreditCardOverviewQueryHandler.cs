using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Application.CreditCard.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Queries.GetOverview
{
    public class GetCreditCardOverviewQueryHandler(ILogger<GetCreditCardOverviewQueryHandler> logger, ICreditCardRepository repository) : IRequestHandler<GetCreditCardOverviewQuery, CreditCardOverviewDto>
    {
        public async Task<CreditCardOverviewDto> Handle(GetCreditCardOverviewQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<Domain.Entities.CreditCard> cards;
            if (request.CreditCardId is { } id)
            {
                var card = await repository.GetWithHistoryAsync(id) ??
                           throw new NotFoundException(nameof(Domain.Entities.CreditCard), id.ToString());
                cards = [card];
            }
            else
            {
                cards = await repository.GetAllWithHistoryAsync(true);
            }

            return CreditCardOverviewCalculator.Build(cards, request.From, request.To, DateOnly.FromDateTime(DateTime.Today));
        }
    }
}

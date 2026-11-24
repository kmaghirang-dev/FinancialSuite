using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Queries.GetAll
{
    public class GetAllCreditCardQueryHandler(ILogger<GetAllCreditCardQueryHandler> logger, IMapper mapper, ICreditCardRepository repository) : IRequestHandler<GetAllCreditCardQuery, IEnumerable<CreditCardDto>>
    {
        public async Task<IEnumerable<CreditCardDto>> Handle(GetAllCreditCardQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllWithHistoryAsync(request.IsActive);
            return data.Select(card => mapper.Map<CreditCardDto>(card).WithLedger(card)).ToList();
        }
    }
}

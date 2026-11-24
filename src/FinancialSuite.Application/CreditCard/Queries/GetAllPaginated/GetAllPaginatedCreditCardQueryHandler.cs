using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.CreditCard.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Queries.GetAllPaginated
{
    public class GetAllPaginatedCreditCardQueryHandler(ILogger<GetAllPaginatedCreditCardQueryHandler> logger, IMapper mapper, ICreditCardRepository repository) : IRequestHandler<GetAllPaginatedCreditCardQuery, PagedResult<CreditCardDto>>
    {
        public async Task<PagedResult<CreditCardDto>> Handle(GetAllPaginatedCreditCardQuery request, CancellationToken cancellationToken)
        {
            var (data, totalCount) = await repository.GetAllMatchingAsync(
                request.IsActive,
                request.SearchPhrase,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            var resultDto = data.Select(card => mapper.Map<CreditCardDto>(card).WithLedger(card)).ToList();

            return new PagedResult<CreditCardDto>(resultDto, totalCount, request.PageSize, request.PageNumber);
        }
    }
}

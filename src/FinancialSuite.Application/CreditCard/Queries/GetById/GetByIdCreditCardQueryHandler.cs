using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCard.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCard.Queries.GetById
{
    public class GetByIdCreditCardQueryHandler(ILogger<GetByIdCreditCardQueryHandler> logger, IMapper mapper, ICreditCardRepository repository) : IRequestHandler<GetByIdCreditCardQuery, CreditCardDto>
    {
        public async Task<CreditCardDto> Handle(GetByIdCreditCardQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetWithHistoryAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCard), id.ToString());

            return mapper.Map<CreditCardDto>(data).WithLedger(data);
        }
    }
}

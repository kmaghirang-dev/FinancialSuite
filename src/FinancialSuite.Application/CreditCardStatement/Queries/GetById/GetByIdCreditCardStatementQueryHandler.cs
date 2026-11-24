using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCardStatement.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardStatement.Queries.GetById
{
    public class GetByIdCreditCardStatementQueryHandler(ILogger<GetByIdCreditCardStatementQueryHandler> logger, IMapper mapper, ICreditCardStatementRepository repository) : IRequestHandler<GetByIdCreditCardStatementQuery, CreditCardStatementDto>
    {
        public async Task<CreditCardStatementDto> Handle(GetByIdCreditCardStatementQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCardStatement), id.ToString());

            return mapper.Map<CreditCardStatementDto>(data);
        }
    }
}

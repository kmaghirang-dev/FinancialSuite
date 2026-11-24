using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.CreditCardPayment.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.CreditCardPayment.Queries.GetById
{
    public class GetByIdCreditCardPaymentQueryHandler(ILogger<GetByIdCreditCardPaymentQueryHandler> logger, IMapper mapper, ICreditCardPaymentRepository repository) : IRequestHandler<GetByIdCreditCardPaymentQuery, CreditCardPaymentDto>
    {
        public async Task<CreditCardPaymentDto> Handle(GetByIdCreditCardPaymentQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(Domain.Entities.CreditCardPayment), id.ToString());

            return mapper.Map<CreditCardPaymentDto>(data);
        }
    }
}

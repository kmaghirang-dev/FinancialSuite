using MediatR;
using FinancialSuite.Application.CreditCardPayment.Dtos;

namespace FinancialSuite.Application.CreditCardPayment.Queries.GetById
{
    public class GetByIdCreditCardPaymentQuery(Guid id) : IRequest<CreditCardPaymentDto>
    {
        public Guid Id { get; } = id;
    }
}

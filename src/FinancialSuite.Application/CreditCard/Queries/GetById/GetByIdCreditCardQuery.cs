using MediatR;
using FinancialSuite.Application.CreditCard.Dtos;

namespace FinancialSuite.Application.CreditCard.Queries.GetById
{
    public class GetByIdCreditCardQuery(Guid id) : IRequest<CreditCardDto>
    {
        public Guid Id { get; } = id;
    }
}

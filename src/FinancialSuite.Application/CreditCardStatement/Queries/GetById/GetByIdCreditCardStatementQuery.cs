using MediatR;
using FinancialSuite.Application.CreditCardStatement.Dtos;

namespace FinancialSuite.Application.CreditCardStatement.Queries.GetById
{
    public class GetByIdCreditCardStatementQuery(Guid id) : IRequest<CreditCardStatementDto>
    {
        public Guid Id { get; } = id;
    }
}

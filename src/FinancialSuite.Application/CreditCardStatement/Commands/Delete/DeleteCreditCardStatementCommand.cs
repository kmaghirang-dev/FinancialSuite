using MediatR;

namespace FinancialSuite.Application.CreditCardStatement.Commands.Delete
{
    public class DeleteCreditCardStatementCommand(Guid id) : IRequest
    {
        public Guid Id { get; } = id;
    }
}

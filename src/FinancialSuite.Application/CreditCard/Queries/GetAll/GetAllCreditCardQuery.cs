using MediatR;
using FinancialSuite.Application.CreditCard.Dtos;

namespace FinancialSuite.Application.CreditCard.Queries.GetAll
{
    public class GetAllCreditCardQuery : IRequest<IEnumerable<CreditCardDto>>
    {
        // Null lists active and archived cards.
        public bool? IsActive { get; set; } = true;
    }
}

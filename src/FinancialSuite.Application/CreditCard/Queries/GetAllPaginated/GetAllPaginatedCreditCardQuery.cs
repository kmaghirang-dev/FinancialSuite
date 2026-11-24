using MediatR;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.CreditCard.Dtos;

namespace FinancialSuite.Application.CreditCard.Queries.GetAllPaginated
{
    public class GetAllPaginatedCreditCardQuery : IRequest<PagedResult<CreditCardDto>>
    {
        // Lists only active records by default; deleting a record sets IsActive = false.
        public bool? IsActive { get; set; } = true;
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}

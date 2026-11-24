using MediatR;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.CreditCardPayment.Dtos;

namespace FinancialSuite.Application.CreditCardPayment.Queries.GetAllPaginated
{
    public class GetAllPaginatedCreditCardPaymentQuery : IRequest<PagedResult<CreditCardPaymentDto>>
    {
        public Guid? CreditCardId { get; set; }
        // Inclusive payment date range; either end may be left open.
        public DateOnly? From { get; set; }
        public DateOnly? To { get; set; }
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}

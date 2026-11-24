using MediatR;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownDetails.Dtos;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetAllPaginated
{
    public class GetAllPaginatedDropDownDetailsQuery : IRequest<PagedResult<GetAllDropDownDetailsDto>>
    {
        public Guid DropDownCategoryId { get; set; }
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}

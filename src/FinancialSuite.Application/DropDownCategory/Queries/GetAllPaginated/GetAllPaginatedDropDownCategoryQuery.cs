using MediatR;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownCategory.Dtos;

namespace FinancialSuite.Application.DropDownCategory.Queries.GetAllPaginated
{
    public class GetAllPaginatedDropDownCategoryQuery : IRequest<PagedResult<GetAllDropDownCategoryDto>>
    {
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}

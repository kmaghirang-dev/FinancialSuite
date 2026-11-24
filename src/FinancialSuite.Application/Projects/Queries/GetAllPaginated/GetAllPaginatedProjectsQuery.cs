using MediatR;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.Projects.Dtos;

namespace FinancialSuite.Application.Projects.Queries.GetAllPaginated
{
    public class GetAllPaginatedProjectsQuery : IRequest<PagedResult<GetAllProjectsDto>>
    {
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }    
    }
}

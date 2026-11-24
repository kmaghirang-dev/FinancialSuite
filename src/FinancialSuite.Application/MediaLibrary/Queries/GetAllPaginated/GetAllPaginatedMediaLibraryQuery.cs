using MediatR;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.MediaLibrary.Dtos;

namespace FinancialSuite.Application.MediaLibrary.Queries.GetAllPaginated
{
    public class GetAllPaginatedMediaLibraryQuery : IRequest<PagedResult<GetAllMediaLibraryDto>>
    {
        public Guid? CategoryId { get; set; }
        public string? SearchPhrase { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SortBy { get; set; }
        public SortDirection SortDirection { get; set; }
    }
}

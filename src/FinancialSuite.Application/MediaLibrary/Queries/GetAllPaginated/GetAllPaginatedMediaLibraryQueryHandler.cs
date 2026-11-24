using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.MediaLibrary.Queries.GetAllPaginated
{
    public class GetAllPaginatedMediaLibraryQueryHandler(ILogger<GetAllPaginatedMediaLibraryQueryHandler> logger, IMapper mapper, IMediaLibraryRepository repository) : IRequestHandler<GetAllPaginatedMediaLibraryQuery, PagedResult<GetAllMediaLibraryDto>>
    {
        public async Task<PagedResult<GetAllMediaLibraryDto>> Handle(GetAllPaginatedMediaLibraryQuery request, CancellationToken cancellationToken)
        {
            var searchPhraseLower = request.SearchPhrase?.ToLower();

            var (data, totalCount) = await repository.GetAllMatchingAsync(
                request.CategoryId,
                searchPhraseLower,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            var resultDto = mapper.Map<IEnumerable<GetAllMediaLibraryDto>>(data);

            var result = new PagedResult<GetAllMediaLibraryDto>(resultDto, totalCount, request.PageSize, request.PageNumber);

            return result;
        }
    }
}

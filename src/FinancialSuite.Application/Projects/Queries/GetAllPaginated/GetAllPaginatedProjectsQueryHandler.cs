using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Projects.Queries.GetAllPaginated
{
    public class GetAllPaginatedProjectsQueryHandler(ILogger<GetAllPaginatedProjectsQueryHandler> logger, IMapper mapper, IProjectsRepository repository) : IRequestHandler<GetAllPaginatedProjectsQuery, PagedResult<GetAllProjectsDto>>
    {
        public async Task<PagedResult<GetAllProjectsDto>> Handle(GetAllPaginatedProjectsQuery request, CancellationToken cancellationToken)
        {
            var searchPhraseLower = request.SearchPhrase?.ToLower();

            var (data, totalCount) = await repository.GetAllMatchingAsync(
                searchPhraseLower,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            var resultDto = mapper.Map<IEnumerable<GetAllProjectsDto>>(data);

            var result = new PagedResult<GetAllProjectsDto>(resultDto, totalCount, request.PageSize, request.PageNumber);

            return result;
        }
    }
}

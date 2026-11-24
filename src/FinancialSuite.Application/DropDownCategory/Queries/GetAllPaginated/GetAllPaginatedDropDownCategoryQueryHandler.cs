using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownCategory.Queries.GetAllPaginated
{
    public class GetAllPaginatedDropDownCategoryQueryHandler(ILogger<GetAllPaginatedDropDownCategoryQueryHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<GetAllPaginatedDropDownCategoryQuery, PagedResult<GetAllDropDownCategoryDto>>
    {
        public async Task<PagedResult<GetAllDropDownCategoryDto>> Handle(GetAllPaginatedDropDownCategoryQuery request, CancellationToken cancellationToken)
        {
            var searchPhraseLower = request.SearchPhrase?.ToLower();

            var (data, totalCount) = await repository.GetAllMatchingAsync(
                searchPhraseLower,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            var resultDto = mapper.Map<IEnumerable<GetAllDropDownCategoryDto>>(data);

            var result = new PagedResult<GetAllDropDownCategoryDto>(resultDto, totalCount, request.PageSize, request.PageNumber);

            return result;
        }
    }
}

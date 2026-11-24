using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetAllPaginated
{
    public class GetAllPaginatedDropDownDetailsQueryHandler(ILogger<GetAllPaginatedDropDownDetailsQueryHandler> logger, IMapper mapper, IDropDownDetailsRepository repository) : IRequestHandler<GetAllPaginatedDropDownDetailsQuery, PagedResult<GetAllDropDownDetailsDto>>
    {
        public async Task<PagedResult<GetAllDropDownDetailsDto>> Handle(GetAllPaginatedDropDownDetailsQuery request, CancellationToken cancellationToken)
        {
            var searchPhraseLower = request.SearchPhrase?.ToLower();

            var (data, totalCount) = await repository.GetAllMatchingAsync(
                request.DropDownCategoryId,
                searchPhraseLower,
                request.PageSize,
                request.PageNumber,
                request.SortBy,
                request.SortDirection);

            var resultDto = mapper.Map<IEnumerable<GetAllDropDownDetailsDto>>(data);

            var result = new PagedResult<GetAllDropDownDetailsDto>(resultDto, totalCount, request.PageSize, request.PageNumber);

            return result;
        }
    }
}

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownCategory.Queries.GetAll
{
    public class GetAllDropDownCategoryQueryHandler(ILogger<GetAllDropDownCategoryQueryHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<GetAllDropDownCategoryQuery, IEnumerable<GetAllDropDownCategoryDto>>
    {
        public async Task<IEnumerable<GetAllDropDownCategoryDto>> Handle(GetAllDropDownCategoryQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            data = data.Where(x => x.IsActive);
            return mapper.Map<IEnumerable<GetAllDropDownCategoryDto>>(data);
        }
    }
}

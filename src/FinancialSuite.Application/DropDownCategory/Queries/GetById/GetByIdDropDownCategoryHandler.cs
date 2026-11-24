using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownCategory.Queries.GetById
{
    public class GetByIdDropDownCategoryHandler(ILogger<GetByIdDropDownCategoryHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<GetByIdDropDownCategoryQuery, GetByIdDropDownCategoryDto?>
    {
        public async Task<GetByIdDropDownCategoryDto?> Handle(GetByIdDropDownCategoryQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetByIdAsync(request.Id);
            data!.DropDownDetails = data.DropDownDetails?.Where(x => x.IsActive).ToList();
            return mapper.Map<GetByIdDropDownCategoryDto?>(data);
        }
    }
}

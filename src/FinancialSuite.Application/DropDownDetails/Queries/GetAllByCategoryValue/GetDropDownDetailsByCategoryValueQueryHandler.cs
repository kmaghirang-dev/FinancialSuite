using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryValue
{
    public class GetDropDownDetailsByCategoryValueQueryHandler(ILogger<GetDropDownDetailsByCategoryValueQueryHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<GetDropDownDetailsByCategoryValueQuery, IEnumerable<GetAllDropDownDetailsDto>>
    {
        public async Task<IEnumerable<GetAllDropDownDetailsDto>> Handle(GetDropDownDetailsByCategoryValueQuery request, CancellationToken cancellationToken)
        {
            var value = request.Value;
            var data = await repository.GetByValueAsync(value);
            if (data == null || !data.IsActive) throw new NotFoundException(nameof(DropDownCategory), value);

            var detailsList = data.DropDownDetails?.Where(x => x.IsActive).ToList();

            var result = mapper.Map<IEnumerable<GetAllDropDownDetailsDto>>(detailsList);

            return result;
        }
    }
}

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryId;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryId
{
    public class GetDropDownDetailsByCategoryIdQueryHandler(ILogger<GetDropDownDetailsByCategoryIdQueryHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<GetDropDownDetailsByCategoryIdQuery, IEnumerable<GetAllDropDownDetailsDto>>
    {
        public async Task<IEnumerable<GetAllDropDownDetailsDto>> Handle(GetDropDownDetailsByCategoryIdQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id);
            if (data == null) throw new NotFoundException(nameof(DropDownCategory), id.ToString());

            var detailsList = data.DropDownDetails?.Where(x => x.IsActive).ToList();

            var result = mapper.Map<IEnumerable<GetAllDropDownDetailsDto>>(detailsList);

            return result;
        }
    }
}

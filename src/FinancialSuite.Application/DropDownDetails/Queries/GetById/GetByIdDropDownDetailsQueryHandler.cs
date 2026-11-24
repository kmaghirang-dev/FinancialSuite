using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Queries.GetById
{
    public class GetByIdDropDownDetailsQueryHandler(
        ILogger<GetByIdDropDownDetailsQueryHandler> logger, 
        IMapper mapper, 
        IDropDownCategoryRepository repository) : IRequestHandler<GetByIdDropDownDetailsQuery, GetByIdDropDownDetailsDto>
    {
        public async Task<GetByIdDropDownDetailsDto> Handle(GetByIdDropDownDetailsQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id);
            if (data == null) throw new NotFoundException(nameof(DropDownCategory), id.ToString());

            var details = data.DropDownDetails.FirstOrDefault(x => x.DropDownDetailId == request.DetailId);
            if (details == null) throw new NotFoundException(nameof(DropDownDetails), request.DetailId.ToString());

            var result = mapper.Map<GetByIdDropDownDetailsDto>(details);

            return result;
        }
    }
}

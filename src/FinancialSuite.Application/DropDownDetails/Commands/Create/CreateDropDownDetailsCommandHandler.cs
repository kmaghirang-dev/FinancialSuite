using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinancialSuite.Domain.Exceptions;

namespace FinancialSuite.Application.DropDownDetails.Commands.Create
{
    public class CreateDropDownDetailsCommandHandler(
        ILogger<CreateDropDownDetailsCommandHandler> logger, 
        IMapper mapper, IDropDownCategoryRepository categoryRepository, IDropDownDetailsRepository detailsRepository) : IRequestHandler<CreateDropDownDetailsCommand, Guid>
    {
        public async Task<Guid> Handle(CreateDropDownDetailsCommand request, CancellationToken cancellationToken)
        {
            var dropDownCategoryId = request.DropDownCategoryId;
            var data = await categoryRepository.GetByIdAsync(dropDownCategoryId);

            if (data == null) throw new NotFoundException(nameof(dropDownCategoryId), dropDownCategoryId.ToString());

            var dropDownDetails = mapper.Map<Domain.Entities.DropDownDetail>(request);

            await detailsRepository.CreateAsync(dropDownDetails);

            return dropDownDetails.DropDownCategoryId;
        }
    }
}

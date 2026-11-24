using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownDetails.Commands.DeleteById;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.Toggle
{
    public class ToggleDropDownDetailsCommandHandler(
        ILogger<DeleteByIdDropDownDetailsCommandHandler> logger,
        IMapper mapper,
        IDropDownCategoryRepository categoryRepository,
        IDropDownDetailsRepository detailsRepository) : IRequestHandler<ToggleDropDownDetailsCommand>
    {
        public async Task Handle(ToggleDropDownDetailsCommand request, CancellationToken cancellationToken)
        {
            var detailId = request.DropDownDetailId;
            var categoryId = request.DropDownCategoryId;

            var data = await categoryRepository.GetByIdAsync(categoryId);

            if (data == null) throw new NotFoundException(nameof(DropDownCategory), categoryId.ToString());

            var details = data.DropDownDetails?.FirstOrDefault(x => x.DropDownDetailId == detailId);

            mapper.Map(request, details);

            await detailsRepository.SaveChanges();
        }
    }
}

using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownDetails.Commands.DeleteAllByCategory;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.DeleteById
{
    public class DeleteByIdDropDownDetailsCommandHandler(
        ILogger<DeleteByIdDropDownDetailsCommandHandler> logger,
        IMapper mapper,
        IDropDownCategoryRepository categoryRepository,
        IDropDownDetailsRepository detailsRepository) : IRequestHandler<DeleteByIdDropDownDetailsCommand>
    {
        public async Task Handle(DeleteByIdDropDownDetailsCommand request, CancellationToken cancellationToken)
        {
            var categoryId = request.CategoryId;
            var detailId = request.DetailId;

            var data = await categoryRepository.GetByIdAsync(categoryId);

            if (data == null) throw new NotFoundException(nameof(DropDownCategory), categoryId.ToString());

            var details = data.DropDownDetails?.Where(x => x.DropDownDetailId == detailId).ToList();

            if (details == null || !details.Any()) throw new NotFoundException(nameof(DropDownDetails), detailId.ToString());

            await detailsRepository.DeleteAsync(details);
        }
    }
}

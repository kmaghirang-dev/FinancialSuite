using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.DeleteAllByCategory
{
    public class DeleteAllDropDownDetailsByCategoryCommandHandler(
        ILogger<DeleteAllDropDownDetailsByCategoryCommandHandler> logger,
        IMapper mapper,
        IDropDownCategoryRepository categoryRepository,
        IDropDownDetailsRepository detailsRepository) : IRequestHandler<DeleteAllDropDownDetailsByCategoryCommand>
    {
        public async Task Handle(DeleteAllDropDownDetailsByCategoryCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await categoryRepository.GetByIdAsync(id);
            if (data == null) throw new NotFoundException(nameof(DropDownCategory), id.ToString());
            await detailsRepository.DeleteAsync(data.DropDownDetails ?? Enumerable.Empty<Domain.Entities.DropDownDetail>());
        }
    }
}

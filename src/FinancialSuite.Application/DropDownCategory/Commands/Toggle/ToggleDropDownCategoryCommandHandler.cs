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

namespace FinancialSuite.Application.DropDownCategory.Commands.Toggle
{
    public class ToggleDropDownCategoryCommandHandler(ILogger<ToggleDropDownCategoryCommandHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<ToggleDropDownCategoryCommand>
    {
        public async Task Handle(ToggleDropDownCategoryCommand request, CancellationToken cancellationToken)
        {
            var id = request.DropDownCategoryId;
            var data = await repository.GetByIdAsync(id) ?? 
                       throw new NotFoundException(nameof(DropDownCategory), id.ToString());

            mapper.Map(request, data);

            await repository.SaveChanges();
        }
    }
}

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

namespace FinancialSuite.Application.DropDownCategory.Commands.Update
{
    public class UpdateDropDownCategoryCommandHandler(ILogger<CreateDropDownCategoryCommandHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<UpdateDropDownCategoryCommand>
    {
        public async Task Handle(UpdateDropDownCategoryCommand request, CancellationToken cancellationToken)
        {
            var id = request.DropDownCategoryId;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(DropDownCategory), id.ToString());

            mapper.Map(request, data);

            await repository.SaveChanges();
        }
    }
}

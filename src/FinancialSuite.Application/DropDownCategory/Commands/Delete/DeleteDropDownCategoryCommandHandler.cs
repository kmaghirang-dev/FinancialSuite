using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownCategory.Commands.Delete
{
    public class DeleteDropDownCategoryCommandHandler(ILogger<DeleteDropDownCategoryCommandHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<DeleteDropDownCategoryCommand>
    {
        public async Task Handle(DeleteDropDownCategoryCommand request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var data = await repository.GetByIdAsync(id) ??
                       throw new NotFoundException(nameof(DropDownCategory), id.ToString());

            await repository.DeleteAsync(data);
        }
    }
}

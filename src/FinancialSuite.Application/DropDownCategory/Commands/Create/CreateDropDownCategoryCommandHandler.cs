using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownCategory.Commands.Create
{
    public class CreateDropDownCategoryCommandHandler (ILogger<CreateDropDownCategoryCommandHandler> logger, IMapper mapper, IDropDownCategoryRepository repository) : IRequestHandler<CreateDropDownCategoryCommand, Guid>
    {
        public async Task<Guid> Handle(CreateDropDownCategoryCommand request, CancellationToken cancellationToken)
        {
            var dropDownCategory = mapper.Map<Domain.Entities.DropDownCategory>(request);
            var id = await repository.CreateAsync(dropDownCategory);
            return id;
        }
    }
}

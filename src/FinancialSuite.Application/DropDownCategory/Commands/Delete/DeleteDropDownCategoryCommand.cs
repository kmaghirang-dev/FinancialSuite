using MediatR;

namespace FinancialSuite.Application.DropDownCategory.Commands.Delete
{
    public class DeleteDropDownCategoryCommand(Guid id) : IRequest
    {
        public Guid Id { get; } = id;
    }
}

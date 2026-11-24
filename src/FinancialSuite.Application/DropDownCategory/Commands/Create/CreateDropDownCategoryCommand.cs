using MediatR;

namespace FinancialSuite.Application.DropDownCategory.Commands.Create
{
    public class CreateDropDownCategoryCommand : IRequest<Guid>
    {
        public string Value { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}

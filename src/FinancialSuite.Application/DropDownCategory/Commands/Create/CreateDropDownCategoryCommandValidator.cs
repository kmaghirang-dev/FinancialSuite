using FluentValidation;

namespace FinancialSuite.Application.DropDownCategory.Commands.Create
{
    public class CreateDropDownCategoryCommandValidator : AbstractValidator<CreateDropDownCategoryCommand>
    {
        public CreateDropDownCategoryCommandValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.CreatedBy)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

using FluentValidation;

namespace FinancialSuite.Application.Projects.Commands.Toggle
{
    public class ToggleProjectCommandValidator : AbstractValidator<ToggleProjectCommand>
    {
        public ToggleProjectCommandValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull();
        }
    }
}

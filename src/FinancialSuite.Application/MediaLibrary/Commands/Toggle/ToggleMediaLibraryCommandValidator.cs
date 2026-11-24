using FluentValidation;

namespace FinancialSuite.Application.MediaLibrary.Commands.Toggle
{
    public class ToggleMediaLibraryCommandValidator : AbstractValidator<ToggleMediaLibraryCommand>
    {
        public ToggleMediaLibraryCommandValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull();
        }
    }
}

using FluentValidation;

namespace FinancialSuite.Application.MediaLibrary.Commands.Create
{
    public class CreateMediaLibraryCommandValidator : AbstractValidator<CreateMediaLibraryCommand>
    {
        public CreateMediaLibraryCommandValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty();

            RuleFor(x => x.StatusId)
                .NotEmpty();

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Details)
                .MaximumLength(1000);
        }
    }
}

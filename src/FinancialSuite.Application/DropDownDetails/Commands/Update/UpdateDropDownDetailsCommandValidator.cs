using FluentValidation;

namespace FinancialSuite.Application.DropDownDetails.Commands.Update
{
    public class UpdateDropDownDetailsCommandValidator : AbstractValidator<UpdateDropDownDetailsCommand>
    {
        public UpdateDropDownDetailsCommandValidator()
        {
            RuleFor(x => x.DisplayValue)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.UpdatedBy)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

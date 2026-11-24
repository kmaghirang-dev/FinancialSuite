using FluentValidation;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.Toggle
{
    internal class ToggleDropDownDetailsCommandValidator : AbstractValidator<ToggleDropDownCategoryCommand>
    {
        public ToggleDropDownDetailsCommandValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull();

            RuleFor(x => x.UpdatedBy)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

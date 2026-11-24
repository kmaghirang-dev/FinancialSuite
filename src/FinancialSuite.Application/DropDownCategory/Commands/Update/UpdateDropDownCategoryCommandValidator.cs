using FluentValidation;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownCategory.Commands.Update
{
    public class UpdateDropDownCategoryCommandValidator : AbstractValidator<UpdateDropDownCategoryCommand>
    {
        public UpdateDropDownCategoryCommandValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.UpdatedBy)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

using FluentValidation;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownDetails.Commands.Create
{
    public class CreateDropDownDetailsCommandValidator : AbstractValidator<CreateDropDownDetailsCommand>
    {
        public CreateDropDownDetailsCommandValidator()
        {
            RuleFor(x => x.DisplayValue)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.CreatedBy)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

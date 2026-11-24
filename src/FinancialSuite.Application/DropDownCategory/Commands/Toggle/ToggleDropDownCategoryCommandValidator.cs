using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Application.DropDownCategory.Commands.Update;
using FinancialSuite.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialSuite.Application.DropDownCategory.Commands.Toggle
{
    public class ToggleDropDownCategoryCommandValidator : AbstractValidator<ToggleDropDownCategoryCommand>
    {
        public ToggleDropDownCategoryCommandValidator()
        {
            RuleFor(x => x.IsActive)
                .NotNull();

            RuleFor(x => x.UpdatedBy)
                .NotEmpty()
                .MaximumLength(100);
        }
    }

}

using System;
using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.Create;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.Create;

[TestSubject(typeof(CreateDropDownDetailsCommandValidator))]
public class CreateDropDownDetailsCommandValidatorTest
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateDropDownDetailsCommand
        {
            DropDownCategoryId = Guid.NewGuid(), Code = "CODE", DisplayValue = "Display Value", CreatedBy = "Creator"
        };

        var validator = new CreateDropDownDetailsCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateDropDownDetailsCommand
        {
            Code = "", DisplayValue = "", CreatedBy = ""
        };

        var validator = new CreateDropDownDetailsCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Code);
        result.ShouldHaveValidationErrorFor(c => c.DisplayValue);
        result.ShouldHaveValidationErrorFor(c => c.CreatedBy);
    }

    [Fact]
    public void Validator_ForTooLongValues_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateDropDownDetailsCommand
        {
            Code = new string('a', 101), DisplayValue = new string('a', 101), CreatedBy = new string('a', 101)
        };

        var validator = new CreateDropDownDetailsCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Code);
        result.ShouldHaveValidationErrorFor(c => c.DisplayValue);
        result.ShouldHaveValidationErrorFor(c => c.CreatedBy);
    }
}

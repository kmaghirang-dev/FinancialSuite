using System;
using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Update;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Update;

[TestSubject(typeof(UpdateDropDownCategoryCommandValidator))]
public class UpdateDropDownCategoryCommandValidatorTest
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateDropDownCategoryCommand
        {
            DropDownCategoryId = Guid.NewGuid(), Value = "Valid Value", UpdatedBy = "Updater"
        };

        var validator = new UpdateDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateDropDownCategoryCommand
        {
            Value = "", UpdatedBy = ""
        };

        var validator = new UpdateDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Value);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedBy);
    }

    [Fact]
    public void Validator_ForTooLongValues_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateDropDownCategoryCommand
        {
            Value = new string('a', 101), UpdatedBy = new string('a', 101)
        };

        var validator = new UpdateDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Value);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedBy);
    }
}

using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Create;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Create;

[TestSubject(typeof(CreateDropDownCategoryCommandValidator))]
public class CreateDropDownCategoryCommandValidatorTest
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateDropDownCategoryCommand
        {
            Value = "Valid Value", CreatedBy = "Creator"
        };

        var validator = new CreateDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateDropDownCategoryCommand
        {
            Value = "", CreatedBy = ""
        };

        var validator = new CreateDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Value);
        result.ShouldHaveValidationErrorFor(c => c.CreatedBy);
    }

    [Fact]
    public void Validator_ForTooLongValues_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new CreateDropDownCategoryCommand
        {
            Value = new string('a', 101), CreatedBy = new string('a', 101)
        };

        var validator = new CreateDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Value);
        result.ShouldHaveValidationErrorFor(c => c.CreatedBy);
    }
}

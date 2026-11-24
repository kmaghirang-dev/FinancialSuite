using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;

namespace FinancialSuite.Application.Tests.DropDownCategory.Commands.Toggle;

[TestSubject(typeof(ToggleDropDownCategoryCommandValidator))]
public class ToggleDropDownCategoryCommandValidatorTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors(bool isActive)
    {
        // Arrange
        var command = new ToggleDropDownCategoryCommand
        {
            UpdatedBy = "Updater",
            IsActive = isActive
        };

        var validator = new ToggleDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyUpdatedBy_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new ToggleDropDownCategoryCommand
        {
            UpdatedBy = ""
        };

        var validator = new ToggleDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdatedBy);
    }

    [Fact]
    public void Validator_ForTooLongUpdatedBy_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new ToggleDropDownCategoryCommand
        {
            UpdatedBy = new string('a', 101)
        };

        var validator = new ToggleDropDownCategoryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.UpdatedBy);
    }
}

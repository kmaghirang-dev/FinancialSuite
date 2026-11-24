using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.DropDownDetails.Commands.Update;

namespace FinancialSuite.Application.Tests.DropDownDetails.Commands.Update;

[TestSubject(typeof(UpdateDropDownDetailsCommandValidator))]
public class UpdateDropDownDetailsCommandValidatorTest
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateDropDownDetailsCommand
        {
            Code = "CODE", DisplayValue = "Display Value", UpdatedBy = "Updater"
        };

        var validator = new UpdateDropDownDetailsCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateDropDownDetailsCommand();

        var validator = new UpdateDropDownDetailsCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Code);
        result.ShouldHaveValidationErrorFor(c => c.DisplayValue);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedBy);
    }

    [Fact]
    public void Validator_ForTooLongValues_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateDropDownDetailsCommand
        {
            Code = new string('a', 101), DisplayValue = new string('a', 101), UpdatedBy = new string('a', 101)
        };

        var validator = new UpdateDropDownDetailsCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Code);
        result.ShouldHaveValidationErrorFor(c => c.DisplayValue);
        result.ShouldHaveValidationErrorFor(c => c.UpdatedBy);
    }
}

using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.Projects.Commands.Update;

namespace FinancialSuite.Application.Tests.Projects.Commands.Update;

[TestSubject(typeof(UpdateProjectCommandValidator))]
public class UpdateProjectCommandValidatorTest
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateProjectCommand
        {
            Name = "Valid Project Name", Description = "Valid Project Description"
        };

        var validator = new UpdateProjectCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateProjectCommand
        {
            Name = ""
        };

        var validator = new UpdateProjectCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validator_ForTooLongValues_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateProjectCommand
        {
            Name = new string('a', 101), Description = new string('a', 501)
        };

        var validator = new UpdateProjectCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
        result.ShouldHaveValidationErrorFor(c => c.Description);
    }
}

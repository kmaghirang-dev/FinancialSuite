using System;
using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.Projects.Commands.Toggle;

namespace FinancialSuite.Application.Tests.Projects.Commands.Toggle;

[TestSubject(typeof(ToggleProjectCommandValidator))]
public class ToggleProjectCommandValidatorTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors(bool isActive)
    {
        // Arrange
        var command = new ToggleProjectCommand
        {
            ProjectId = Guid.NewGuid(),
            IsActive = isActive
        };

        var validator = new ToggleProjectCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

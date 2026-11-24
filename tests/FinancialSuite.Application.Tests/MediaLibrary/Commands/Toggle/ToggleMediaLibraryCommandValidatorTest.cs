using System;
using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Toggle;

namespace FinancialSuite.Application.Tests.MediaLibrary.Commands.Toggle;

[TestSubject(typeof(ToggleMediaLibraryCommandValidator))]
public class ToggleMediaLibraryCommandValidatorTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors(bool isActive)
    {
        // Arrange
        var command = new ToggleMediaLibraryCommand
        {
            MediaId = Guid.NewGuid(),
            IsActive = isActive
        };

        var validator = new ToggleMediaLibraryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

using System;
using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Update;

namespace FinancialSuite.Application.Tests.MediaLibrary.Commands.Update;

[TestSubject(typeof(UpdateMediaLibraryCommandValidator))]
public class UpdateMediaLibraryCommandValidatorTest
{
    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateMediaLibraryCommand
        {
            CategoryId = Guid.NewGuid(), StatusId = Guid.NewGuid(), Title = "Valid Title", Details = "Valid Details"
        };

        var validator = new UpdateMediaLibraryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateMediaLibraryCommand
        {
            CategoryId = Guid.Empty, StatusId = Guid.Empty, Title = ""
        };

        var validator = new UpdateMediaLibraryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.CategoryId);
        result.ShouldHaveValidationErrorFor(c => c.StatusId);
        result.ShouldHaveValidationErrorFor(c => c.Title);
    }

    [Fact]
    public void Validator_ForTooLongValues_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateMediaLibraryCommand
        {
            CategoryId = Guid.NewGuid(), StatusId = Guid.NewGuid(), Title = new string('a', 201), Details = new string('a', 1001)
        };

        var validator = new UpdateMediaLibraryCommandValidator();

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Title);
        result.ShouldHaveValidationErrorFor(c => c.Details);
    }
}

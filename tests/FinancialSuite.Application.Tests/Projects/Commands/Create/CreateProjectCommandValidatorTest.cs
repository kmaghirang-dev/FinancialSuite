using FluentAssertions;
using FluentValidation.TestHelper;
using JetBrains.Annotations;
using FinancialSuite.Application.Projects.Commands.Create;
using Xunit;

namespace FinancialSuite.Application.Tests.Projects.Commands.Create;

[TestSubject(typeof(CreateProjectCommandValidator))]
public class CreateProjectCommandValidatorTest
{

    [Fact]
    public void Validator_ForValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange

        var command = new CreateProjectCommand
        {
            Name = "Valid Project Name",
            Description = "Valid Project Description"
        };

        var validator = new CreateProjectCommandValidator();

        // Act

        var result = validator.TestValidate(command);

        // Assert

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForInvalidCommand_ShouldHaveValidationErrors()
    {
        // Arrange

        var command = new CreateProjectCommand
        {
            Name = "",
            Description = ""
        };

        var validator = new CreateProjectCommandValidator();

        // Act

        var result = validator.TestValidate(command);

        // Assert

        result.ShouldHaveValidationErrorFor(c => c.Name);
    }
}
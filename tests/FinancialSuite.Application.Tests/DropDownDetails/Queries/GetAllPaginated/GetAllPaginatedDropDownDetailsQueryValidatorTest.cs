using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllPaginated;

namespace FinancialSuite.Application.Tests.DropDownDetails.Queries.GetAllPaginated;

[TestSubject(typeof(GetAllPaginatedDropDownDetailsQueryValidator))]
public class GetAllPaginatedDropDownDetailsQueryValidatorTest
{
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(30)]
    public void Validator_ForValidQuery_ShouldNotHaveValidationErrors(int pageSize)
    {
        // Arrange
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            PageNumber = 1,
            PageSize = pageSize,
            SortBy = "Code",
            SortDirection = SortDirection.Ascending
        };

        var validator = new GetAllPaginatedDropDownDetailsQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForQueryWithoutSortBy_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var validator = new GetAllPaginatedDropDownDetailsQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validator_ForInvalidPageNumber_ShouldHaveValidationErrors(int pageNumber)
    {
        // Arrange
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        var validator = new GetAllPaginatedDropDownDetailsQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(100)]
    public void Validator_ForInvalidPageSize_ShouldHaveValidationErrors(int pageSize)
    {
        // Arrange
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        var validator = new GetAllPaginatedDropDownDetailsQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validator_ForInvalidSortBy_ShouldHaveValidationErrors()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "InvalidColumn"
        };

        var validator = new GetAllPaginatedDropDownDetailsQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.SortBy);
    }

    [Fact]
    public void Validator_ForInvalidSortDirection_ShouldHaveValidationErrors()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "Code",
            SortDirection = (SortDirection)99
        };

        var validator = new GetAllPaginatedDropDownDetailsQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.SortDirection);
    }
}

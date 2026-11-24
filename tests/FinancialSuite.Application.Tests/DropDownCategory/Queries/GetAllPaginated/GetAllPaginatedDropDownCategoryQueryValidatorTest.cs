using FluentValidation.TestHelper;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownCategory.Queries.GetAllPaginated;

namespace FinancialSuite.Application.Tests.DropDownCategory.Queries.GetAllPaginated;

[TestSubject(typeof(GetAllPaginatedDropDownCategoryQueryValidator))]
public class GetAllPaginatedDropDownCategoryQueryValidatorTest
{
    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(30)]
    public void Validator_ForValidQuery_ShouldNotHaveValidationErrors(int pageSize)
    {
        // Arrange
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            PageNumber = 1,
            PageSize = pageSize,
            SortBy = "Value",
            SortDirection = SortDirection.Ascending
        };

        var validator = new GetAllPaginatedDropDownCategoryQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ForQueryWithoutSortBy_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        var validator = new GetAllPaginatedDropDownCategoryQueryValidator();

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
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            PageNumber = pageNumber,
            PageSize = 10
        };

        var validator = new GetAllPaginatedDropDownCategoryQueryValidator();

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
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            PageNumber = 1,
            PageSize = pageSize
        };

        var validator = new GetAllPaginatedDropDownCategoryQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.PageSize);
    }

    [Fact]
    public void Validator_ForInvalidSortBy_ShouldHaveValidationErrors()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "InvalidColumn"
        };

        var validator = new GetAllPaginatedDropDownCategoryQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.SortBy);
    }

    [Fact]
    public void Validator_ForInvalidSortDirection_ShouldHaveValidationErrors()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            PageNumber = 1,
            PageSize = 10,
            SortBy = "Value",
            SortDirection = (SortDirection)99
        };

        var validator = new GetAllPaginatedDropDownCategoryQueryValidator();

        // Act
        var result = validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(q => q.SortDirection);
    }
}

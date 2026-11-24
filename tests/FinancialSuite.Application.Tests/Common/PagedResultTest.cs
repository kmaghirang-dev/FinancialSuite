using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;
using FinancialSuite.Application.Common;

namespace FinancialSuite.Application.Tests.Common;

[TestSubject(typeof(PagedResult<>))]
public class PagedResultTest
{
    [Theory]
    [InlineData(12, 5, 1, 3, 1, 5)]
    [InlineData(12, 5, 2, 3, 6, 10)]
    [InlineData(12, 5, 3, 3, 11, 15)]
    [InlineData(10, 10, 1, 1, 1, 10)]
    [InlineData(0, 10, 1, 0, 1, 10)]
    public void Constructor_ForGivenValues_CalculatesPagingProperties(
        int totalCount, int pageSize, int pageNumber, int expectedTotalPages, int expectedItemsFrom, int expectedItemsTo)
    {
        // Arrange
        var items = new List<string> { "a", "b" };

        // Act
        var result = new PagedResult<string>(items, totalCount, pageSize, pageNumber);

        // Assert
        result.Items.Should().BeSameAs(items);
        result.TotalCount.Should().Be(totalCount);
        result.PageSize.Should().Be(pageSize);
        result.PageNumber.Should().Be(pageNumber);
        result.TotalPages.Should().Be(expectedTotalPages);
        result.ItemsFrom.Should().Be(expectedItemsFrom);
        result.ItemsTo.Should().Be(expectedItemsTo);
    }
}

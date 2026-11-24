using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.DropDownCategory.Dtos;
using FinancialSuite.Application.DropDownCategory.Queries.GetAllPaginated;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownCategory.Queries.GetAllPaginated;

[TestSubject(typeof(GetAllPaginatedDropDownCategoryQueryHandler))]
public class GetAllPaginatedDropDownCategoryQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownCategoryRepository> _repositoryMock;
    private readonly GetAllPaginatedDropDownCategoryQueryHandler _handler;

    public GetAllPaginatedDropDownCategoryQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllPaginatedDropDownCategoryQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownCategoryRepository>();
        _handler = new GetAllPaginatedDropDownCategoryQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var query = new GetAllPaginatedDropDownCategoryQuery
        {
            SearchPhrase = "SeArCh",
            PageNumber = 2,
            PageSize = 5,
            SortBy = "Value",
            SortDirection = SortDirection.Descending
        };

        IEnumerable<Domain.Entities.DropDownCategory> data = new List<Domain.Entities.DropDownCategory> { new Domain.Entities.DropDownCategory(), new Domain.Entities.DropDownCategory() };
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync("search", 5, 2, "Value", SortDirection.Descending))
            .ReturnsAsync((data, 12));

        var dtos = new List<GetAllDropDownCategoryDto> { new GetAllDropDownCategoryDto(), new GetAllDropDownCategoryDto() };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllDropDownCategoryDto>>(data))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeSameAs(dtos);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.ItemsFrom.Should().Be(6);
        result.ItemsTo.Should().Be(10);
    }
}

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
using FinancialSuite.Application.DropDownDetails.Dtos;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllPaginated;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.DropDownDetails.Queries.GetAllPaginated;

[TestSubject(typeof(GetAllPaginatedDropDownDetailsQueryHandler))]
public class GetAllPaginatedDropDownDetailsQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IDropDownDetailsRepository> _repositoryMock;
    private readonly GetAllPaginatedDropDownDetailsQueryHandler _handler;

    public GetAllPaginatedDropDownDetailsQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllPaginatedDropDownDetailsQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IDropDownDetailsRepository>();
        _handler = new GetAllPaginatedDropDownDetailsQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetAllPaginatedDropDownDetailsQuery
        {
            DropDownCategoryId = categoryId,
            SearchPhrase = "SeArCh",
            PageNumber = 2,
            PageSize = 5,
            SortBy = "Code",
            SortDirection = SortDirection.Descending
        };

        IEnumerable<DropDownDetail> data = new List<DropDownDetail> { new DropDownDetail(), new DropDownDetail() };
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync(categoryId, "search", 5, 2, "Code", SortDirection.Descending))
            .ReturnsAsync((data, 12));

        var dtos = new List<GetAllDropDownDetailsDto> { new GetAllDropDownDetailsDto(), new GetAllDropDownDetailsDto() };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllDropDownDetailsDto>>(data))
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

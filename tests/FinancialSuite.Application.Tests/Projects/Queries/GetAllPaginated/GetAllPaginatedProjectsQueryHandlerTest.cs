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
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Application.Projects.Queries.GetAllPaginated;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.Projects.Queries.GetAllPaginated;

[TestSubject(typeof(GetAllPaginatedProjectsQueryHandler))]
public class GetAllPaginatedProjectsQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProjectsRepository> _repositoryMock;
    private readonly GetAllPaginatedProjectsQueryHandler _handler;

    public GetAllPaginatedProjectsQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllPaginatedProjectsQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IProjectsRepository>();
        _handler = new GetAllPaginatedProjectsQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var query = new GetAllPaginatedProjectsQuery
        {
            SearchPhrase = "SeArCh",
            PageNumber = 2,
            PageSize = 5,
            SortBy = "Name",
            SortDirection = SortDirection.Descending
        };

        IEnumerable<Domain.Entities.Project> data = new List<Domain.Entities.Project> { new Domain.Entities.Project(), new Domain.Entities.Project() };
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync("search", 5, 2, "Name", SortDirection.Descending))
            .ReturnsAsync((data, 12));

        var dtos = new List<GetAllProjectsDto> { new GetAllProjectsDto(), new GetAllProjectsDto() };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllProjectsDto>>(data))
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

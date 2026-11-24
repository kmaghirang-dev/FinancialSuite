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
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Application.MediaLibrary.Queries.GetAllPaginated;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Queries.GetAllPaginated;

[TestSubject(typeof(GetAllPaginatedMediaLibraryQueryHandler))]
public class GetAllPaginatedMediaLibraryQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly GetAllPaginatedMediaLibraryQueryHandler _handler;

    public GetAllPaginatedMediaLibraryQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllPaginatedMediaLibraryQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        _handler = new GetAllPaginatedMediaLibraryQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidQuery_ReturnsPagedResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var query = new GetAllPaginatedMediaLibraryQuery
        {
            CategoryId = categoryId,
            SearchPhrase = "SeArCh",
            PageNumber = 2,
            PageSize = 5,
            SortBy = "Title",
            SortDirection = SortDirection.Descending
        };

        IEnumerable<Domain.Entities.MediaLibrary> data = new List<Domain.Entities.MediaLibrary> { new Domain.Entities.MediaLibrary(), new Domain.Entities.MediaLibrary() };
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync(categoryId, "search", 5, 2, "Title", SortDirection.Descending))
            .ReturnsAsync((data, 12));

        var dtos = new List<GetAllMediaLibraryDto> { new GetAllMediaLibraryDto(), new GetAllMediaLibraryDto() };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllMediaLibraryDto>>(data))
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

    [Fact]
    public async Task Handle_WithoutCategoryId_PassesNullCategoryToRepository()
    {
        // Arrange
        var query = new GetAllPaginatedMediaLibraryQuery { PageNumber = 1, PageSize = 10 };

        IEnumerable<Domain.Entities.MediaLibrary> data = new List<Domain.Entities.MediaLibrary>();
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync(null, null, 10, 1, null, It.IsAny<SortDirection?>()))
            .ReturnsAsync((data, 0));
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllMediaLibraryDto>>(data))
            .Returns(new List<GetAllMediaLibraryDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(0);
        _repositoryMock.Verify(repo => repo.GetAllMatchingAsync(null, null, 10, 1, null, It.IsAny<SortDirection?>()), Times.Once);
    }
}

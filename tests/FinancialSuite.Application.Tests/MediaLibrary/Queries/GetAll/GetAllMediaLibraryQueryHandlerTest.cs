using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Application.MediaLibrary.Queries.GetAll;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Queries.GetAll;

[TestSubject(typeof(GetAllMediaLibraryQueryHandler))]
public class GetAllMediaLibraryQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly GetAllMediaLibraryQueryHandler _handler;

    public GetAllMediaLibraryQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetAllMediaLibraryQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        _handler = new GetAllMediaLibraryQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForActiveAndInactiveRecords_ReturnsActiveRecordsOnly()
    {
        // Arrange
        var active = new Domain.Entities.MediaLibrary { MediaId = Guid.NewGuid(), IsActive = true };
        var inactive = new Domain.Entities.MediaLibrary { MediaId = Guid.NewGuid(), IsActive = false };

        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Domain.Entities.MediaLibrary> { active, inactive });

        var dtos = new List<GetAllMediaLibraryDto> { new GetAllMediaLibraryDto { MediaId = active.MediaId } };
        _mapperMock.Setup(mapper => mapper.Map<IEnumerable<GetAllMediaLibraryDto>>(
                It.Is<IEnumerable<Domain.Entities.MediaLibrary>>(data => data.Count() == 1 && data.Single() == active)))
            .Returns(dtos);

        // Act
        var result = await _handler.Handle(new GetAllMediaLibraryQuery(), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dtos);
    }
}

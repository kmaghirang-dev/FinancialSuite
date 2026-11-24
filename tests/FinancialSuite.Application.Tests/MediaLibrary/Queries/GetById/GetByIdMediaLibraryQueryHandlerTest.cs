using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Dtos;
using FinancialSuite.Application.MediaLibrary.Queries.GetById;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Queries.GetById;

[TestSubject(typeof(GetByIdMediaLibraryQueryHandler))]
public class GetByIdMediaLibraryQueryHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly GetByIdMediaLibraryQueryHandler _handler;

    public GetByIdMediaLibraryQueryHandlerTest()
    {
        var loggerMock = new Mock<ILogger<GetByIdMediaLibraryQueryHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        _handler = new GetByIdMediaLibraryQueryHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingMediaLibrary_ReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new Domain.Entities.MediaLibrary { MediaId = id };
        var dto = new GetByIdMediaLibraryDto { MediaId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);
        _mapperMock.Setup(mapper => mapper.Map<GetByIdMediaLibraryDto>(data))
            .Returns(dto);

        // Act
        var result = await _handler.Handle(new GetByIdMediaLibraryQuery(id), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Handle_WithNonExistingMediaLibrary_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        // Act
        Func<Task> act = () => _handler.Handle(new GetByIdMediaLibraryQuery(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The MediaLibrary with ID {id} was not found.");
    }
}

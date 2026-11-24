using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Delete;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Commands.Delete;

[TestSubject(typeof(DeleteMediaLibraryCommandHandler))]
public class DeleteMediaLibraryCommandHandlerTest
{
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly DeleteMediaLibraryCommandHandler _handler;

    public DeleteMediaLibraryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<DeleteMediaLibraryCommandHandler>>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        _handler = new DeleteMediaLibraryCommandHandler(
            loggerMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingMediaLibrary_DeletesMediaLibrary()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new Domain.Entities.MediaLibrary { MediaId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        // Act
        await _handler.Handle(new DeleteMediaLibraryCommand(id), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(repo => repo.DeleteAsync(data), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingMediaLibrary_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteMediaLibraryCommand(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The MediaLibrary with ID {id} was not found.");
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.MediaLibrary>()), Times.Never);
    }
}

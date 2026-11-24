using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Toggle;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Commands.Toggle;

[TestSubject(typeof(ToggleMediaLibraryCommandHandler))]
public class ToggleMediaLibraryCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly ToggleMediaLibraryCommandHandler _handler;

    public ToggleMediaLibraryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<ToggleMediaLibraryCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        var userContextMock = new Mock<IUserContext>();
        var currentUser = new CurrentUser("OwnerId", "OwnerUserName", ["Owner"], null);
        userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);
        _handler = new ToggleMediaLibraryCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            userContextMock.Object,
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_UpdatesMediaLibrary()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleMediaLibraryCommand
        {
            MediaId = id,
            IsActive = false
        };

        var data = new Domain.Entities.MediaLibrary { MediaId = id, IsActive = true };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(mapper => mapper.Map(command, data), Times.Once);
        data.UpdatedBy.Should().Be("OwnerUserName");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingMediaLibrary_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleMediaLibraryCommand { MediaId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.MediaLibrary)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The MediaLibrary with ID {id} was not found.");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }
}

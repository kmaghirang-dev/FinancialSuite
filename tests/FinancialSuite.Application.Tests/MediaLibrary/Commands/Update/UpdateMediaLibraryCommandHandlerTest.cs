using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.MediaLibrary.Commands.Update;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.MediaLibrary.Commands.Update;

[TestSubject(typeof(UpdateMediaLibraryCommandHandler))]
public class UpdateMediaLibraryCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMediaLibraryRepository> _repositoryMock;
    private readonly UpdateMediaLibraryCommandHandler _handler;

    public UpdateMediaLibraryCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<UpdateMediaLibraryCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IMediaLibraryRepository>();
        var userContextMock = new Mock<IUserContext>();
        var currentUser = new CurrentUser("OwnerId", "OwnerUserName", ["Owner"], null);
        userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);
        _handler = new UpdateMediaLibraryCommandHandler(
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
        var command = new UpdateMediaLibraryCommand
        {
            MediaId = id,
            CategoryId = Guid.NewGuid(),
            StatusId = Guid.NewGuid(),
            Title = "Test Title",
            Season = 1,
            EpisodeNo = 2,
            Details = "Test Details",
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
        var command = new UpdateMediaLibraryCommand { MediaId = id };

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

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.Projects.Commands.Delete;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Interfaces;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.Projects.Commands.Delete;

[TestSubject(typeof(DeleteProjectCommandHandler))]
public class DeleteProjectCommandHandlerTest
{
    private readonly Mock<IProjectsRepository> _repositoryMock;
    private readonly Mock<IProjectAuthorizationService> _projectAuthorizationServiceMock;
    private readonly DeleteProjectCommandHandler _handler;

    public DeleteProjectCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<DeleteProjectCommandHandler>>();
        _repositoryMock = new Mock<IProjectsRepository>();
        _projectAuthorizationServiceMock = new Mock<IProjectAuthorizationService>();
        _handler = new DeleteProjectCommandHandler(
            loggerMock.Object,
            _repositoryMock.Object,
            _projectAuthorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingProject_DeletesProject()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new Domain.Entities.Project { ProjectId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(data, ResourceOperation.Delete))
            .Returns(true);

        // Act
        await _handler.Handle(new DeleteProjectCommand(id), CancellationToken.None);

        // Assert
        _repositoryMock.Verify(repo => repo.DeleteAsync(data), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingProject_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.Project)null);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteProjectCommand(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The Projects with ID {id} was not found.");
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Project>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldThrowForbidException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var data = new Domain.Entities.Project { ProjectId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(data, ResourceOperation.Delete))
            .Returns(false);

        // Act
        Func<Task> act = () => _handler.Handle(new DeleteProjectCommand(id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbidException>();
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Project>()), Times.Never);
    }
}

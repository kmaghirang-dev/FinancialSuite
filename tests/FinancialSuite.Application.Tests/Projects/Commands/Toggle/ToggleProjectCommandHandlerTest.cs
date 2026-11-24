using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.Projects.Commands.Toggle;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Interfaces;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.Tests.Projects.Commands.Toggle;

[TestSubject(typeof(ToggleProjectCommandHandler))]
public class ToggleProjectCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProjectsRepository> _repositoryMock;
    private readonly Mock<IProjectAuthorizationService> _projectAuthorizationServiceMock;
    private readonly ToggleProjectCommandHandler _handler;

    public ToggleProjectCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<ToggleProjectCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _repositoryMock = new Mock<IProjectsRepository>();
        var userContextMock = new Mock<IUserContext>();
        var currentUser = new CurrentUser("OwnerId", "OwnerUserName", ["Owner"], null);
        userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);
        _projectAuthorizationServiceMock = new Mock<IProjectAuthorizationService>();
        _handler = new ToggleProjectCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            userContextMock.Object,
            _repositoryMock.Object,
            _projectAuthorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_UpdatesProject()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleProjectCommand
        {
            ProjectId = id,
            IsActive = false
        };

        var data = new Domain.Entities.Project { ProjectId = id, IsActive = true };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(data, ResourceOperation.Update))
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mapperMock.Verify(mapper => mapper.Map(command, data), Times.Once);
        data.UpdatedBy.Should().Be("OwnerUserName");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingProject_ShouldThrowNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleProjectCommand { ProjectId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Domain.Entities.Project)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The Projects with ID {id} was not found.");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldThrowForbidException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new ToggleProjectCommand { ProjectId = id };
        var data = new Domain.Entities.Project { ProjectId = id };

        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(data);

        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(data, ResourceOperation.Update))
            .Returns(false);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbidException>();
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }
}

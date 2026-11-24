using AutoMapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.Application.Projects.Commands.Create;
using FinancialSuite.Application.Projects.Commands.Update;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Interfaces;
using Xunit;

namespace FinancialSuite.Application.Tests.Projects.Commands.Update;

[TestSubject(typeof(UpdateProjectCommandHandler))]
public class UpdateProjectCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProjectsRepository> _projectRepositoryMock;
    private readonly Mock<IProjectAuthorizationService> _projectAuthorizationServiceMock;
    private readonly UpdateProjectCommandHandler _handler;

    public UpdateProjectCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<UpdateProjectCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _projectRepositoryMock = new Mock<IProjectsRepository>();
        var userContextMock = new Mock<IUserContext>();
        var currentUser = new CurrentUser("OwnerId", "OwnerUserName", ["Owner"], null);
        userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);
        _projectAuthorizationServiceMock = new Mock<IProjectAuthorizationService>();
        _handler = new UpdateProjectCommandHandler(
            loggerMock.Object, 
            _mapperMock.Object, 
            userContextMock.Object, 
            _projectRepositoryMock.Object,
            _projectAuthorizationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_ReturnsProjectId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateProjectCommand
        {
            ProjectId = id,
            Name = "Updated Project Name",
            Description = "Updated Project Description"
        };

        var project = new Project()
        {
            ProjectId = id,
            Name = "Project Name",
            Description = "Project Description"

        };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);

        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Update))
            .Returns(true);


        // Act

        await _handler.Handle(command, CancellationToken.None);

        // Assert

        _projectRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map(command, project), Times.Once);

    }

    [Fact]
    public async Task Handle_WithNonExistingRestaurant_ShouldThrowNoFoundException()
    {
        // ARRANGE  

        var id = Guid.NewGuid();
        var request = new UpdateProjectCommand
        {
            ProjectId = id,
        };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Project)null);

        // ACT

        Func<Task> act = () => _handler.Handle(request, CancellationToken.None);

        // ASSERT

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"The Projects with ID {id} was not found.");
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldThrowNForbiddenException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var command = new UpdateProjectCommand
        {
            ProjectId = id,
        };

        var project = new Project()
        {
            ProjectId = id,
        };

        _projectRepositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);

        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Update))
            .Returns(false);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbidException>();    
    }
}
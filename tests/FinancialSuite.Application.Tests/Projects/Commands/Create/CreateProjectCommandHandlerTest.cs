using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.Application.Projects.Commands.Create;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Repositories;
using Xunit;
using FluentAssertions;

namespace FinancialSuite.Application.Tests.Projects.Commands.Create;

[TestSubject(typeof(CreateProjectCommandHandler))]
public class CreateProjectCommandHandlerTest
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IProjectsRepository> _projectRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;
    private readonly CreateProjectCommandHandler _handler;

    public CreateProjectCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<CreateProjectCommandHandler>>();
        _mapperMock = new Mock<IMapper>();
        _projectRepositoryMock = new Mock<IProjectsRepository>();
        _userContextMock = new Mock<IUserContext>();
        _handler = new CreateProjectCommandHandler(
            loggerMock.Object,
            _mapperMock.Object,
            _userContextMock.Object,
            _projectRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ForValidCommand_ReturnsProjectId()
    {
        // Arrange

        var command = new CreateProjectCommand();
        var project = new Project();

        _mapperMock.Setup(m => m.Map<Project>(command))
            .Returns(project);

        var expectedId = Guid.NewGuid();
        _projectRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Project>()))
            .ReturnsAsync(expectedId);

        var currentUser = new CurrentUser("OwnerId", "OwnerUserName", ["Owner"], null);
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);

        // ACT

        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert

        result.Should().Be(expectedId);
        project.OwnerId.Should().Be("OwnerId");
        project.CreatedBy.Should().Be("OwnerUserName");
        project.UpdatedBy.Should().Be("OwnerUserName");
        _projectRepositoryMock.Verify(repo => repo.CreateAsync(project), Times.Once);
    }


}

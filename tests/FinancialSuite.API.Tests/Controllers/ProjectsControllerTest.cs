using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using FinancialSuite.API.Controllers;
using FinancialSuite.Application.Projects.Commands.Create;
using FinancialSuite.Application.Projects.Commands.Toggle;
using FinancialSuite.Application.Projects.Commands.Update;
using FinancialSuite.Application.Projects.Dtos;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Interfaces;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(ProjectsController))]
public class ProjectsControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IProjectsRepository> _repositoryMock =  new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IProjectAuthorizationService> _projectAuthorizationServiceMock = new();

    public ProjectsControllerTest(WebApplicationFactory<Program> factory)
    {
        var currentUser = new CurrentUser("1", "test@example.com", [UserRoles.SuperAdmin], null);
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IPolicyEvaluator, FakePolicyEvaluator>());
                services.Replace(ServiceDescriptor.Scoped(typeof(IProjectsRepository), _ => _repositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IUserContext), _ => _userContextMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IProjectAuthorizationService), _ => _projectAuthorizationServiceMock.Object));
            });
        });
    }

    [Fact]
    public async Task GetAll_ForValidRequest_Returns200Ok()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/projects/paginated?pageNumber=1&pageSize=10", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllPaginated_ForInvalidRequest_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/projects/paginated", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ForNonExistingId_ShouldReturn404NotFound()
    {
        // Arrange
        var id = Guid.Parse("00000000-0000-0000-0000-000000000000");

        _repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Project)null);

        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/projects/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task GetById_ForExistingId_ShouldReturn200Ok()
    {
        // Arrange
        var id = Guid.NewGuid();

        var project = new Project
        {
            ProjectId = id,
            Name = "Test Project",
            Description = "Test Description"
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(project);

        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/projects/{id}", TestContext.Current.CancellationToken);
        var returnedProject =  await result.Content.ReadFromJsonAsync<GetByIdProjectDto>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returnedProject.Should().NotBeNull();
        returnedProject!.ProjectId.Should().Be(id);
        returnedProject.Name.Should().Be("Test Project");
        returnedProject.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task GetAllActive_ForValidRequest_Returns200Ok()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Project> { new Project { ProjectId = Guid.NewGuid(), IsActive = true } });
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/projects", TestContext.Current.CancellationToken);
        var returned = await result.Content.ReadFromJsonAsync<List<GetAllProjectsDto>>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        returned.Should().ContainSingle();
    }

    [Fact]
    public async Task Create_ForValidCommand_Returns201Created()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Project>()))
            .ReturnsAsync(id);
        var client = _factory.CreateClient();

        var command = new CreateProjectCommand { Name = "Test Project", Description = "Test Description" };

        // Act
        var result = await client.PostAsJsonAsync("api/projects", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Headers.Location!.ToString().Should().EndWith($"/api/projects/{id}");
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Project>()), Times.Once);
    }

    [Fact]
    public async Task Create_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        var command = new CreateProjectCommand { Name = "" };

        // Act
        var result = await client.PostAsJsonAsync("api/projects", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async Task Update_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var project = new Project { ProjectId = id };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);
        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Update))
            .Returns(true);
        var client = _factory.CreateClient();

        var command = new UpdateProjectCommand { Name = "Updated Project", Description = "Updated Description" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/projects/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Update_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Project)null);
        var client = _factory.CreateClient();

        var command = new UpdateProjectCommand { Name = "Updated Project" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/projects/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ForUnauthorizedUser_Returns403Forbidden()
    {
        // Arrange
        var id = Guid.NewGuid();
        var project = new Project { ProjectId = id };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);
        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Update))
            .Returns(false);
        var client = _factory.CreateClient();

        var command = new UpdateProjectCommand { Name = "Updated Project" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/projects/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task Update_ForInvalidCommand_Returns400BadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var client = _factory.CreateClient();

        var command = new UpdateProjectCommand { Name = "" };

        // Act
        var result = await client.PatchAsJsonAsync($"api/projects/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Toggle_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var project = new Project { ProjectId = id, IsActive = true };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);
        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Update))
            .Returns(true);
        var client = _factory.CreateClient();

        var command = new ToggleProjectCommand { IsActive = false };

        // Act
        var result = await client.PatchAsJsonAsync($"api/projects/toggle/{id}", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Delete_ForExistingId_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var project = new Project { ProjectId = id };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);
        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Delete))
            .Returns(true);
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/projects/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _repositoryMock.Verify(repo => repo.DeleteAsync(project), Times.Once);
    }

    [Fact]
    public async Task Delete_ForNonExistingId_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync((Project)null);
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/projects/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ForUnauthorizedUser_Returns403Forbidden()
    {
        // Arrange
        var id = Guid.NewGuid();
        var project = new Project { ProjectId = id };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(id))
            .ReturnsAsync(project);
        _projectAuthorizationServiceMock.Setup(auth => auth.Authorize(project, ResourceOperation.Delete))
            .Returns(false);
        var client = _factory.CreateClient();

        // Act
        var result = await client.DeleteAsync($"api/projects/{id}", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Project>()), Times.Never);
    }
}

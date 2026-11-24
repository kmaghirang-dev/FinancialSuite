using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using FinancialSuite.API.Controllers;
using FinancialSuite.Application.Users;
using FinancialSuite.Application.Users.Commands.AssignUserRole;
using FinancialSuite.Application.Users.Commands.UnassignUserRole;
using FinancialSuite.Application.Users.Commands.UpdateUserDetails;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(IdentityController))]
public class IdentityControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Mock<IUserStore<User>> _userStoreMock = new();
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;

    public IdentityControllerTest(WebApplicationFactory<Program> factory)
    {
        var currentUser = new CurrentUser("1", "test@example.com", [UserRoles.SuperAdmin], null);
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IPolicyEvaluator, FakePolicyEvaluator>());
                services.Replace(ServiceDescriptor.Scoped(typeof(IUserContext), _ => _userContextMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IUserStore<User>), _ => _userStoreMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(UserManager<User>), _ => _userManagerMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(RoleManager<IdentityRole>), _ => _roleManagerMock.Object));
            });
        });
    }

    [Fact]
    public async Task UpdateUserDetails_ForExistingUser_Returns204NoContent()
    {
        // Arrange
        var dbUser = new User { Id = "1" };
        _userStoreMock.Setup(store => store.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbUser);
        var client = _factory.CreateClient();

        var command = new UpdateUserDetailsCommand { DateOfBirth = new DateOnly(1990, 1, 1), Nationality = "Filipino" };

        // Act
        var result = await client.PatchAsJsonAsync("api/identity/user", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        dbUser.Nationality.Should().Be("Filipino");
        _userStoreMock.Verify(store => store.UpdateAsync(dbUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserDetails_ForNonExistingUser_Returns404NotFound()
    {
        // Arrange
        _userStoreMock.Setup(store => store.FindByIdAsync("1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);
        var client = _factory.CreateClient();

        var command = new UpdateUserDetailsCommand { Nationality = "Filipino" };

        // Act
        var result = await client.PatchAsJsonAsync("api/identity/user", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignUserRole_ForExistingUserAndRole_Returns204NoContent()
    {
        // Arrange
        var user = new User { Email = "user@example.com" };
        _userManagerMock.Setup(um => um.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(rm => rm.FindByNameAsync(UserRoles.Admin))
            .ReturnsAsync(new IdentityRole(UserRoles.Admin));
        var client = _factory.CreateClient();

        var command = new AssignUserRoleCommand { UserEmail = "user@example.com", Role = UserRoles.Admin };

        // Act
        var result = await client.PatchAsJsonAsync("api/identity/userRole", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _userManagerMock.Verify(um => um.AddToRoleAsync(user, UserRoles.Admin), Times.Once);
    }

    [Fact]
    public async Task AssignUserRole_ForNonExistingUser_Returns404NotFound()
    {
        // Arrange
        _userManagerMock.Setup(um => um.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((User)null);
        var client = _factory.CreateClient();

        var command = new AssignUserRoleCommand { UserEmail = "missing@example.com", Role = UserRoles.Admin };

        // Act
        var result = await client.PatchAsJsonAsync("api/identity/userRole", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnassignUserRole_ForExistingUserAndRole_Returns204NoContent()
    {
        // Arrange
        var user = new User { Email = "user@example.com" };
        _userManagerMock.Setup(um => um.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(rm => rm.FindByNameAsync(UserRoles.Admin))
            .ReturnsAsync(new IdentityRole(UserRoles.Admin));
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Delete, "api/identity/userRole")
        {
            Content = JsonContent.Create(new UnassignUserRoleCommand { UserEmail = "user@example.com", Role = UserRoles.Admin })
        };

        // Act
        var result = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        _userManagerMock.Verify(um => um.RemoveFromRoleAsync(user, UserRoles.Admin), Times.Once);
    }

    [Fact]
    public async Task UnassignUserRole_ForNonExistingRole_Returns404NotFound()
    {
        // Arrange
        _userManagerMock.Setup(um => um.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(new User { Email = "user@example.com" });
        _roleManagerMock.Setup(rm => rm.FindByNameAsync("MissingRole"))
            .ReturnsAsync((IdentityRole)null);
        var client = _factory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Delete, "api/identity/userRole")
        {
            Content = JsonContent.Create(new UnassignUserRoleCommand { UserEmail = "user@example.com", Role = "MissingRole" })
        };

        // Act
        var result = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

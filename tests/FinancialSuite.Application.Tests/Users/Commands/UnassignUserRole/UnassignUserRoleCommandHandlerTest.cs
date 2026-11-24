using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.Users.Commands.AssignUserRole;
using FinancialSuite.Application.Users.Commands.UnassignUserRole;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;

namespace FinancialSuite.Application.Tests.Users.Commands.UnassignUserRole;

[TestSubject(typeof(UnassignUserRoleCommandHandler))]
public class UnassignUserRoleCommandHandlerTest
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly UnassignUserRoleCommandHandler _handler;

    public UnassignUserRoleCommandHandlerTest()
    {
        // The handler's logger is typed with AssignUserRoleCommandHandler.
        var loggerMock = new Mock<ILogger<AssignUserRoleCommandHandler>>();
        _userManagerMock = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
        _handler = new UnassignUserRoleCommandHandler(
            loggerMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingUserAndRole_RemovesRole()
    {
        // Arrange
        var command = new UnassignUserRoleCommand { UserEmail = "user@example.com", Role = UserRoles.Admin };
        var user = new User { Email = "user@example.com" };
        var role = new IdentityRole(UserRoles.Admin);

        _userManagerMock.Setup(um => um.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(rm => rm.FindByNameAsync(UserRoles.Admin))
            .ReturnsAsync(role);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _userManagerMock.Verify(um => um.RemoveFromRoleAsync(user, UserRoles.Admin), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UnassignUserRoleCommand { UserEmail = "missing@example.com", Role = UserRoles.Admin };

        _userManagerMock.Setup(um => um.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((User)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("The User with ID missing@example.com was not found.");
        _userManagerMock.Verify(um => um.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistingRole_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UnassignUserRoleCommand { UserEmail = "user@example.com", Role = "MissingRole" };

        _userManagerMock.Setup(um => um.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(new User { Email = "user@example.com" });
        _roleManagerMock.Setup(rm => rm.FindByNameAsync("MissingRole"))
            .ReturnsAsync((IdentityRole)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("The IdentityRole with ID MissingRole was not found.");
        _userManagerMock.Verify(um => um.RemoveFromRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
}

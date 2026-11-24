using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FinancialSuite.Application.Users;
using FinancialSuite.Application.Users.Commands.UpdateUserDetails;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;

namespace FinancialSuite.Application.Tests.Users.Commands.UpdateUserDetails;

[TestSubject(typeof(UpdateUserDetailsCommandHandler))]
public class UpdateUserDetailsCommandHandlerTest
{
    private readonly Mock<IUserStore<User>> _userStoreMock;
    private readonly UpdateUserDetailsCommandHandler _handler;

    public UpdateUserDetailsCommandHandlerTest()
    {
        var loggerMock = new Mock<ILogger<UpdateUserDetailsCommandHandler>>();
        var userContextMock = new Mock<IUserContext>();
        var currentUser = new CurrentUser("UserId", "user@example.com", ["User"], null);
        userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(currentUser);
        _userStoreMock = new Mock<IUserStore<User>>();
        _handler = new UpdateUserDetailsCommandHandler(
            loggerMock.Object,
            userContextMock.Object,
            _userStoreMock.Object);
    }

    [Fact]
    public async Task Handle_ForExistingUser_UpdatesUserDetails()
    {
        // Arrange
        var command = new UpdateUserDetailsCommand
        {
            DateOfBirth = new DateOnly(1990, 1, 1),
            Nationality = "Filipino"
        };
        var dbUser = new User { Id = "UserId" };

        _userStoreMock.Setup(store => store.FindByIdAsync("UserId", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbUser);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        dbUser.DateOfBirth.Should().Be(new DateOnly(1990, 1, 1));
        dbUser.Nationality.Should().Be("Filipino");
        _userStoreMock.Verify(store => store.UpdateAsync(dbUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateUserDetailsCommand { Nationality = "Filipino" };

        _userStoreMock.Setup(store => store.FindByIdAsync("UserId", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("The User with ID UserId was not found.");
        _userStoreMock.Verify(store => store.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

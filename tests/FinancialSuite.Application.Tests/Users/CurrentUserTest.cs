using FluentAssertions;
using JetBrains.Annotations;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using Xunit;

namespace FinancialSuite.Application.Tests.Users;

[TestSubject(typeof(CurrentUser))]
public class CurrentUserTest
{

    [Fact]
    public void IsInRoleTest_WithMatchingRole_ShouldReturnTrue()
    {
        // Arrange
        var currentUser = new CurrentUser("1", "userName", [UserRoles.SuperAdmin], null);

        // Act
        var result = currentUser.IsInRole(UserRoles.SuperAdmin);

        // Assert
        Assert.True(result);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInRoleTest_WithMatchingRole_ShouldReturnFalse()
    {
        // Arrange
        var currentUser = new CurrentUser("1", "userName", [UserRoles.SuperAdmin], null);

        // Act
        var result = currentUser.IsInRole(UserRoles.Admin);

        // Assert
        Assert.False(result);
        result.Should().BeFalse();
    }
}
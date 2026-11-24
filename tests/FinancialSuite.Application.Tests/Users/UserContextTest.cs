using System;
using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Moq;
using FinancialSuite.Application.Users;
using Xunit;

namespace FinancialSuite.Application.Tests.Users;

[TestSubject(typeof(UserContext))]
public class UserContextTest
{
            
    [Fact]
    public void GetCurrentUserTest_WithAuthenticatedUser_ShouldReturnCurrentUser()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "123"),
            new Claim(ClaimTypes.Name, "John Doe"),
            new Claim(ClaimTypes.Email, "john.doe@example.com"),
            new Claim("DateOfBirth", "1990-01-01")
        };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "authenticated"))
        };
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var userContext = new UserContext(httpContextAccessorMock.Object);

        // Act
        var currentUser = userContext.GetCurrentUser();

        // Assert
        Assert.NotNull(currentUser);
    }

    [Fact]
    public void GetCurrentUserTest_WithUserContextNotPresent_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
        var userContext = new UserContext(httpContextAccessorMock.Object);

        // Act
        Action action = () => userContext.GetCurrentUser();

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("User Context not present");
    }
}
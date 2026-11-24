using System;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.API.Middlewares;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Middlewares;

[TestSubject(typeof(ErrorHandlingMiddleware))]
public class ErrorHandlingMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync_WhenNoExceptionThrown_ShouldCallNextDelegate()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var errorLogRepositoryMock = new Mock<IErrorLogRepository>();
        var middleware = new ErrorHandlingMiddleware(loggerMock.Object, errorLogRepositoryMock.Object);
        
        var nextDelegateMock =  new Mock<RequestDelegate>();

        // Act

        await middleware.InvokeAsync(context, nextDelegateMock.Object);

        // Assert

        nextDelegateMock.Verify(next => next.Invoke(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundExceptionThrown_ShouldSetStatusCode404()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var errorLogRepositoryMock = new Mock<IErrorLogRepository>();
        var middleware = new ErrorHandlingMiddleware(loggerMock.Object, errorLogRepositoryMock.Object);

        var exception = new NotFoundException(nameof(Project), "1");

        // Act
        await middleware.InvokeAsync(context, _ => throw exception);


        // Assert

        context.Response.StatusCode.Should().Be(404);
    }


    [Fact]
    public async Task InvokeAsync_WhenForbidExceptionThrown_ShouldSetStatusCode403()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var errorLogRepositoryMock = new Mock<IErrorLogRepository>();
        var middleware = new ErrorHandlingMiddleware(loggerMock.Object, errorLogRepositoryMock.Object);

        var exception = new ForbidException();

        // Act
        await middleware.InvokeAsync(context, _ => throw exception);

        // Assert
        context.Response.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_ShouldSetStatusCode500()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        var errorLogRepositoryMock = new Mock<IErrorLogRepository>();
        var middleware = new ErrorHandlingMiddleware(loggerMock.Object, errorLogRepositoryMock.Object);

        var exception = new Exception();

        // Act
        await middleware.InvokeAsync(context, _ => throw exception);

        // Assert
        context.Response.StatusCode.Should().Be(500);

    }
}
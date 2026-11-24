using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.API.Middlewares;
using Xunit;

namespace FinancialSuite.API.Tests.Middlewares;

[TestSubject(typeof(RequestTimeLoggingMiddleware))]
public class RequestTimeLoggingMiddlewareTest
{
    [Fact]
    public async Task InvokeAsync_ForFastRequest_ShouldCallNextAndNotLog()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var loggerMock = new Mock<ILogger<RequestTimeLoggingMiddleware>>();
        var middleware = new RequestTimeLoggingMiddleware(loggerMock.Object);

        var nextDelegateMock = new Mock<RequestDelegate>();

        // Act
        await middleware.InvokeAsync(context, nextDelegateMock.Object);

        // Assert
        nextDelegateMock.Verify(next => next.Invoke(context), Times.Once);
        loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never);
    }
}

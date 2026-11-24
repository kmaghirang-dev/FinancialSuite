using System.Diagnostics;

namespace FinancialSuite.API.Middlewares
{
    public class RequestTimeLoggingMiddleware(ILogger<RequestTimeLoggingMiddleware> logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            await next.Invoke(context);
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds/1000 > 4)
            {
                logger.LogInformation("Request [{verb}] at {path} took longer than 4 seconds ({time}s).", context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds/1000);
            }
        }
    }
}

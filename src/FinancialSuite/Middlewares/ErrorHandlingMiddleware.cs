using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.API.Middlewares
{
    public class ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger,
        IErrorLogRepository errorLogRepository) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (NotFoundException notFound)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(notFound.Message);
                logger.LogWarning(notFound.Message);
            }
            catch (ForbidException)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Access denied.");
                logger.LogWarning("Access denied.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                await errorLogRepository.CreateAsync(new ErrorLog
                {
                    Message = ex.Message,
                    Exception = ex.ToString(),
                    RequestMethod = context.Request.Method,
                    RequestPath = context.Request.Path,
                    DateCreated = DateTime.Now
                });

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An internal error occurred.");
            }
        }
    }
}

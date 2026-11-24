using Microsoft.AspNetCore.Http;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Interfaces;

namespace FinancialSuite.Infrastructure.Authorization.Services
{
    public class DataOwnershipContext(IHttpContextAccessor httpContextAccessor, IUserContext userContext) : IDataOwnershipContext
    {
        private int systemOperations;

        public string? CurrentUserEmail => httpContextAccessor.HttpContext == null ? null : userContext.GetCurrentUser()?.UserName;

        // Seeders and background services run outside a request, so they are system operations.
        public bool IsSystemOperation => systemOperations > 0 || httpContextAccessor.HttpContext == null;

        public bool IsOwner => string.Equals(CurrentUserEmail, DataOwnership.OwnerEmail, StringComparison.OrdinalIgnoreCase);

        public IDisposable BeginSystemOperation()
        {
            systemOperations++;
            return new SystemOperation(this);
        }

        private sealed class SystemOperation(DataOwnershipContext context) : IDisposable
        {
            private bool disposed;

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                context.systemOperations--;
            }
        }
    }
}

namespace FinancialSuite.Domain.Interfaces
{
    public interface IDataOwnershipContext
    {
        // Email of the signed-in user whose records are visible; null when no user is signed in.
        string? CurrentUserEmail { get; }

        // True for seeders, background services and code running inside BeginSystemOperation: no ownership rules apply.
        bool IsSystemOperation { get; }

        bool IsOwner { get; }

        IDisposable BeginSystemOperation();
    }
}

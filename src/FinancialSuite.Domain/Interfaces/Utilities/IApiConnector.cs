namespace FinancialSuite.Domain.Interfaces.Utilities
{
    public interface IApiConnector
    {
        // Endpoints are absolute URLs, or relative to the base address of the HttpClient the connector was created with.
        // Non-success responses throw HttpRequestException.
        Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken);
        Task DeleteAsync(string endpoint, CancellationToken cancellationToken);
    }
}

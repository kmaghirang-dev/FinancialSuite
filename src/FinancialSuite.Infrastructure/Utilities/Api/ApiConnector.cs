using System.Net.Http.Json;
using FinancialSuite.Domain.Interfaces.Utilities;

namespace FinancialSuite.Infrastructure.Utilities.Api
{
    public class ApiConnector(HttpClient httpClient) : IApiConnector
    {
        public async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken) =>
            await httpClient.GetFromJsonAsync<TResponse>(endpoint, cancellationToken);

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken)
        {
            using var response = await httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body, CancellationToken cancellationToken)
        {
            using var response = await httpClient.PutAsJsonAsync(endpoint, body, cancellationToken);
            return await ReadResponseAsync<TResponse>(response, cancellationToken);
        }

        public async Task DeleteAsync(string endpoint, CancellationToken cancellationToken)
        {
            using var response = await httpClient.DeleteAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        private static async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentLength == 0) return default;

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
        }
    }
}

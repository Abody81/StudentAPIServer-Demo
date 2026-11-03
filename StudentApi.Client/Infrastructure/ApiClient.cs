using StudentApi.Client.Services.Interfaces;
using StudentApi.Client.State;
using System.Net.Http.Headers;

namespace StudentApi.Client.Infrastructure;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AppState _state;
    private readonly ITokenRefreshService _refreshService;

    public ApiClient(HttpClient httpClient, AppState state, ITokenRefreshService refreshService)
    {
        _httpClient = httpClient;
        _state = state;
        _refreshService = refreshService;

        UpdateAuthorizationHeader();
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await SendWithAutoRefreshAsync(() => _httpClient.GetAsync(url));
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T data)
    {
        return await SendWithAutoRefreshAsync(() => _httpClient.PostAsJsonAsync(url, data));
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T data)
    {
        return await SendWithAutoRefreshAsync(() => _httpClient.PutAsJsonAsync(url, data));
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await SendWithAutoRefreshAsync(() => _httpClient.DeleteAsync(url));
    }

    private async Task<HttpResponseMessage> SendWithAutoRefreshAsync(Func<Task<HttpResponseMessage>> sendRequest)
    {
        UpdateAuthorizationHeader();

        var response = await sendRequest();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await _refreshService.RefreshAsync();

            if (!refreshed)
            {
                throw new InvalidOperationException("Session expired. Please login again.");
            }

            UpdateAuthorizationHeader();
            response = await sendRequest();
        }

        return response;
    }

    private void UpdateAuthorizationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            !string.IsNullOrEmpty(_state.AccessToken)
                ? new AuthenticationHeaderValue("Bearer", _state.AccessToken)
                : null;
    }
}

using StudentApi.Client.State;
using StudentApi.Shared.Requests;
using StudentApi.Shared.Responses;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using StudentApi.Client.Services.Interfaces;

namespace StudentApi.Client.Services;

public class TokenRefreshService : ITokenRefreshService
{
    private readonly HttpClient _httpClient;
    private readonly AppState _state;

    public TokenRefreshService(HttpClient httpClient, AppState state)
    {
        _httpClient = httpClient;
        _state = state;
    }

    public async Task<bool> RefreshAsync()
    {
        if (string.IsNullOrEmpty(_state.RefreshToken))
        {
            return false;
        }

        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/refresh",
            new RefreshRequest(_state.UserId, _state.RefreshToken));

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var refreshData = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (refreshData is null)
        {
            return false;
        }

        _state.AccessToken = refreshData.AccessToken;
        _state.RefreshToken = refreshData.RefreshToken;
        _state.UserId = refreshData.UserId;
        _state.TokenIssuedAt = DateTime.UtcNow;

        return true;
    }
}

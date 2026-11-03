using StudentApi.Client.State;
using StudentApi.Shared.Requests;
using StudentApi.Shared.Responses;
using System.Net.Http.Json;

using StudentApi.Client.Services.Interfaces;

namespace StudentApi.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AppState _state;

    public AuthService(HttpClient httpClient, AppState state)
    {
        _httpClient = httpClient;
        _state = state;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/auth/login",
            new LoginRequest(email, password));

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var loginData = await response.Content.ReadFromJsonAsync<LoginResponse>();

        if (loginData is null)
        {
            return false;
        }

        _state.AccessToken = loginData.AccessToken;
        _state.RefreshToken = loginData.RefreshToken;
        _state.UserId = loginData.UserId;
        _state.TokenIssuedAt = DateTime.UtcNow;

        return true;
    }

    public async Task LogoutAsync()
    {
        if (string.IsNullOrEmpty(_state.RefreshToken))
        {
            return;
        }

        await _httpClient.PostAsJsonAsync(
            "api/auth/logout",
            new LogoutRequest(_state.UserId, _state.RefreshToken));

        _state.AccessToken = null;
        _state.RefreshToken = null;
        _state.UserId = 0;
    }
}

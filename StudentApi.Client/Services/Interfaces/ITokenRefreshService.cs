namespace StudentApi.Client.Services.Interfaces;

public interface ITokenRefreshService
{
    Task<bool> RefreshAsync();
}

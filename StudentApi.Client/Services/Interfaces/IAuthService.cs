namespace StudentApi.Client.Services.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
}

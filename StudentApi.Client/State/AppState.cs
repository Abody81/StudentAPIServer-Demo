namespace StudentApi.Client.State;

public class AppState
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int UserId { get; set; }
    public DateTime TokenIssuedAt { get; set; }
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public TimeSpan RemainingTokenTime =>
        TimeSpan.FromMinutes(AccessTokenLifetimeMinutes) - (DateTime.UtcNow - TokenIssuedAt);

    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);
}

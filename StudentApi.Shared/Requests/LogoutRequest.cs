namespace StudentApi.Shared.Requests;

public record LogoutRequest(string Email, string RefreshToken);

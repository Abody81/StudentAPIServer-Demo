namespace StudentShared.Responses;

public record LoginResponse(string AccessToken, string RefreshToken, Guid UserId);

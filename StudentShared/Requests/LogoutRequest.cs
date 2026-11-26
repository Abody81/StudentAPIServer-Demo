namespace StudentShared.Requests;

public record LogoutRequest(Guid Id, string RefreshToken);

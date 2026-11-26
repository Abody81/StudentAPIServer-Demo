namespace StudentShared.Requests;

public record RefreshRequest(Guid Id, string RefreshToken);

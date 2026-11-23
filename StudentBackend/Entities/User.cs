namespace StudentBackend.Entities;

/// <summary>
/// Authentication identity: "who are you and how do you log in?"
/// An Admin is a User with Role = Admin and NO Student row.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>Unique across the system (enforced by a DB unique constraint).</summary>
    public string Email { get; set; } = null!;

    /// <summary>BCrypt hash. Touched only by the auth endpoints (login/refresh/logout).</summary>
    public string PasswordHash { get; set; } = null!;

    /// <summary>"Admin" / "Student" - case-sensitive when checked by [Authorize].</summary>
    public string Role { get; set; } = null!;

    // --- Refresh token state ---
    // One session per account for now. A separate RefreshTokens table
    // (multi-device sessions) is a parked future task.
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenRevokedAt { get; set; }

    /// <summary>Academic profile. Null for the Admin (one-to-zero-or-one).</summary>
    public Student? Student { get; set; }
}

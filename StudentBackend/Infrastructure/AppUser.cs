using Microsoft.AspNetCore.Identity;
using StudentBackend.Entities;

namespace StudentBackend.Infrastructure;

// Authentication identity, backed by ASP.NET Core Identity.
// Lives in Infrastructure (not the domain): it inherits a framework class
public class AppUser : IdentityUser<int>
{
    // --- Refresh token state (custom: Identity does not manage JWT refresh) ---
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime? RefreshTokenRevokedAt { get; set; }

    /// <summary>Academic profile (shared PK: Student.StudentId == AppUser.Id). Null for the Admin.</summary>
    public Student? Student { get; set; }
}

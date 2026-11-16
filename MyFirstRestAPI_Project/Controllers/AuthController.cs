using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentApi.DataSimulation;
using StudentApi.Shared.Requests;
using System.Security.Cryptography;
using StudentApi.Shared.Responses;
using MyFirstRestAPI_Project.Models;
using Microsoft.Extensions.Options;
using MyFirstRestAPI_Project.Settings;
using Microsoft.AspNetCore.RateLimiting;

namespace MyFirstRestAPI_Project.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("ApiLimiter")]
public class AuthController(IOptions<JwtSettings> jwtSettings, ILogger<AuthController> logger) : ControllerBase
{
    private readonly JwtSettings _jwt = jwtSettings.Value;

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    // Privacy: mask the local part of emails (S@g.com -> s***@g.com).
    // First character + domain stay recognizable so attack patterns
    // (same target hammered) remain visible in the logs.
    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0) return "***";   // no local part / no '@' at all
        return $"{email[..2]}***{email[at..]}";
    }

    private LoginResponse IssueNewToken(Student student)
    {
        var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),

                new Claim(ClaimTypes.Email, student.Email),

                new Claim(ClaimTypes.Role, student.Role)
            };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.Key));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpireMinutes),
                signingCredentials: creds
            );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GenerateRefreshToken();

        // Store refresh token securely (hash + expiry + not revoked)
        student.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
        student.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpireDays);
        student.RefreshTokenRevokedAt = null;

        return new LoginResponse
        (
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            UserId: student.Id
        );
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthLimiter")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        // Audit rule: log identifiers and the outcome only - NEVER the
        // attempted password or the issued tokens. Emails are MASKED
        // (s***@g.com): the first character and domain stay recognizable so
        // attack patterns remain visible, without spreading full PII
        // through logs, backups and external sinks.
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var student = StudentDataSimulation.StudentsList
            .FirstOrDefault(s => s.Email == request.Email);

        if (student == null)
        {
            logger.LogWarning("Login FAILED: unknown email {MaskedEmail} from {IP}", MaskEmail(request.Email), ip);
            return Unauthorized("Invalid credentials");
        }

        bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash);

        if (!isValidPassword)
        {
            logger.LogWarning("Login FAILED: invalid password for user {UserId} ({MaskedEmail}) from {IP}",
                student.Id, MaskEmail(student.Email), ip);
            return Unauthorized("Invalid credentials");
        }

        logger.LogInformation("Login OK for user {UserId} from {IP}", student.Id, ip);
        return Ok(IssueNewToken(student));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("AuthLimiter")]
    public ActionResult<LoginResponse> Refresh([FromBody] RefreshRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var student = StudentDataSimulation.StudentsList
            .FirstOrDefault(s => s.Id == request.Id);

        if (student == null)
        {
            logger.LogWarning("Refresh FAILED: unknown user {UserId} from {IP}", request.Id, ip);
            return Unauthorized("Invalid refresh request");
        }

        if (student.RefreshTokenRevokedAt != null)
        {
            // Using a token that was already revoked at logout is suspicious.
            logger.LogWarning("Refresh FAILED: revoked token for user {UserId} from {IP}", student.Id, ip);
            return Unauthorized("Refresh token is revoked");
        }

        if (student.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            logger.LogWarning("Refresh FAILED: expired token for user {UserId} from {IP}", student.Id, ip);
            return Unauthorized("Refresh token expired");
        }

        bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
        if (!refreshValid)
        {
            logger.LogWarning("Refresh FAILED: invalid token for user {UserId} from {IP}", student.Id, ip);
            return Unauthorized("Invalid refresh token");
        }

        // Issue NEW access token (same claims & signing settings as login)
        logger.LogInformation("Refresh OK: token rotated for user {UserId} from {IP}", student.Id, ip);
        return Ok(IssueNewToken(student));
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var student = StudentDataSimulation.StudentsList
            .FirstOrDefault(s => s.Id == request.Id);

        if (student == null)
            return Ok(); // Do not reveal if user exists

        bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
        if (!refreshValid)
            return Ok();

        student.RefreshTokenRevokedAt = DateTime.UtcNow;

        // A logout from an unusual IP is itself a security signal (stolen
        // token being killed), so the IP belongs here too.
        logger.LogInformation("Logout for user {UserId} from {IP}: refresh token revoked",
            student.Id, ip);

        return Ok();
    }
}

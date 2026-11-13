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
public class AuthController(IOptions<JwtSettings> jwtSettings) : ControllerBase
{
    private readonly JwtSettings _jwt = jwtSettings.Value;

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
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
        var student = StudentDataSimulation.StudentsList
            .FirstOrDefault(s => s.Email == request.Email);

        if (student == null)
            return Unauthorized("Invalid credentials");

        bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, student.PasswordHash);

        if (!isValidPassword)
            return Unauthorized("Invalid credentials");

        return Ok(IssueNewToken(student));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("AuthLimiter")]
    public ActionResult<LoginResponse> Refresh([FromBody] RefreshRequest request)
    {
        var student = StudentDataSimulation.StudentsList
            .FirstOrDefault(s => s.Id == request.Id);

        if (student == null)
            return Unauthorized("Invalid refresh request");

        if (student.RefreshTokenRevokedAt != null)
            return Unauthorized("Refresh token is revoked");

        if (student.RefreshTokenExpiresAt <= DateTime.UtcNow)
            return Unauthorized("Refresh token expired");

        bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
        if (!refreshValid)
            return Unauthorized("Invalid refresh token");

        // Issue NEW access token (same claims & signing settings as login)
        return Ok(IssueNewToken(student));
    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] LogoutRequest request)
    {
        var student = StudentDataSimulation.StudentsList
            .FirstOrDefault(s => s.Id == request.Id);

        if (student == null)
            return Ok(); // Do not reveal if user exists

        bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, student.RefreshTokenHash);
        if (!refreshValid)
            return Ok();

        student.RefreshTokenRevokedAt = DateTime.UtcNow;
        
        return Ok();
    }
}

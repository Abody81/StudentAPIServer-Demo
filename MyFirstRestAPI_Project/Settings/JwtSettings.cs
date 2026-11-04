using System.ComponentModel.DataAnnotations;

namespace MyFirstRestAPI_Project.Settings;

public class JwtSettings
{
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenExpireMinutes { get; set; }

    [Range(1, 30)]
    public int RefreshTokenExpireDays { get; set; } = 7;
}
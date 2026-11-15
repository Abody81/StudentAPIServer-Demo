using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.IdentityModel.Tokens;
using MyFirstRestAPI_Project.Authorization;
using MyFirstRestAPI_Project.Settings;

namespace MyFirstRestAPI_Project.Extensions;

/// <summary>
/// JWT authentication: settings binding + validation, bearer token
/// validation parameters, and the StudentOwnerOrAdmin authorization policy.
/// </summary>
public static class JwtAuthenticationExtensions
{
    public static void AddJwtAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<JwtSettings>()
            .Bind(builder.Configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("StudentOwnerOrAdmin", policy =>
                policy.Requirements.Add(new StudentOwnerOrAdminRequirement()));
        });

        // Audit-log every 401/403 issued by [Authorize] BEFORE controllers run
        // (replaces the default handler - our wrapper delegates to it).
        builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler,
            LoggingAuthorizationMiddlewareResultHandler>();

        builder.Services.AddSingleton<IAuthorizationHandler, StudentOwnerOrAdminHandler>();
    }
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyFirstRestAPI_Project.Authorization;
using MyFirstRestAPI_Project.Settings;
using Scalar.AspNetCore;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddControllers();

builder.Services.AddHealthChecks();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,

            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter: Bearer {your JWT token}"
        };

        var components = document.Components ??= new OpenApiComponents();
        components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = securityScheme
        };

        var reference = new OpenApiSecuritySchemeReference("Bearer", document, null);
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [reference] = new List<string>()
        });

        return Task.CompletedTask;
    });
});

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

builder.Services.AddSingleton<IAuthorizationHandler, StudentOwnerOrAdminHandler>();

builder.Services.AddRateLimiter(options =>
{
    // Called by the rate-limiting middleware the moment a request is rejected.
    // It replaces the old custom middleware that inspected the status code
    // after the whole pipeline had already run.
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.", cancellationToken);
    };

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // FIXED window -> SLIDING window:
        // the fixed window forgets everything at the window boundary, allowing a
        // burst of ~2x the limit in seconds (verified by RateLimitProbe).
        // The sliding window remembers the last 60 seconds, closing that hole.
        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: ip,
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 5,
                QueueLimit = 0
            });
    });

    // General/light data endpoints (reserved for future use): token bucket
    // per authenticated user. The burst ceiling (TokenLimit=20) absorbs the
    // legitimate burst of an app starting up, while the steady refill
    // (1 token / 2s = 30/min) caps sustained abuse.
    options.AddPolicy("ApiLimiter", httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var key = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: key,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 15,
                TokensPerPeriod = 1,
                ReplenishmentPeriod = TimeSpan.FromSeconds(2),
                AutoReplenishment = true,
                QueueLimit = 0
            });
    });

    // HEAVY endpoints (e.g. GetAllStudents - full unbounded list): a tight budget
    // of ~10 requests per 5 minutes that CANNOT be spent in one burst.
    // Token bucket: the bucket size (TokenLimit) caps the instantaneous burst
    // at 2, while the steady refill (1 token per 30s) sustains ~10 per 5 min.
    // Idling never accumulates more than TokenLimit tokens.
    options.AddPolicy("HeavyLimiter", httpContext =>
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var key = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetTokenBucketLimiter(
            partitionKey: key,
            factory: _ => new TokenBucketRateLimiterOptions
            {
                TokenLimit = 2,
                TokensPerPeriod = 1,
                ReplenishmentPeriod = TimeSpan.FromSeconds(25),
                AutoReplenishment = true,
                QueueLimit = 0
            });
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
// Rate limiter must run AFTER authentication: the ApiLimiter policy partitions
// by authenticated user id, which is only available once auth has populated
// httpContext.User.
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

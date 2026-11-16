using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace MyFirstRestAPI_Project.Extensions;

/// <summary>
/// Three rate-limiting policies:
/// - AuthLimiter:  sliding window per IP on login/refresh (anti brute-force).
/// - ApiLimiter:   token bucket per user for general endpoints.
/// - HeavyLimiter: tight token bucket per user for heavy endpoints.
/// Every rejection (429) is logged as a security signal.
/// </summary>
public static class RateLimiterExtensions
{
    public static void AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                var ctx = context.HttpContext;

                // Audit: a rejected request is a security signal (brute force / abuse),
                // so it is logged, not just answered. The ILogger is resolved from
                // RequestServices because this callback runs outside normal DI.
                var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
                var role = ctx.User.FindFirstValue(ClaimTypes.Role);
                var identity = role is null ? userId : $"{userId} ({role})";
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                logger.LogWarning("Rate limit: {Method} {Path} rejected for user {Identity} from {IP}",
                    ctx.Request.Method, ctx.Request.Path, identity, ip);

                ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await ctx.Response.WriteAsync(
                    "Too many requests. Please try again later.", cancellationToken);
            };

            options.AddPolicy("AuthLimiter", httpContext =>
            {
                string ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

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

            options.AddPolicy("ApiLimiter", httpContext =>
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var key = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: key,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 12,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(3),
                        AutoReplenishment = true,
                        QueueLimit = 0
                    });
            });

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
    }
}

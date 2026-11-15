using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace MyFirstRestAPI_Project.Authorization;

/// <summary>
/// Wraps the default authorization middleware result handler to audit-log
/// every 401/403 issued by [Authorize] on endpoints. These rejections happen
/// BEFORE any controller code runs, so controllers cannot see them.
///
/// Layer separation (no double logging):
/// - 401/403 from [Authorize] attributes  -> logged HERE (middleware layer).
/// - Validation / NotFound / Forbid()     -> logged in the CONTROLLERS
///   (they have the business context). Controller Forbid() does NOT pass
///   through this handler, and [AllowAnonymous] endpoints never hit it.
/// </summary>
public sealed class LoggingAuthorizationMiddlewareResultHandler(
    ILogger<LoggingAuthorizationMiddlewareResultHandler> logger)
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _inner = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded)
        {
            // Challenged = 401 (missing/invalid/expired token, anonymous user).
            // Forbidden  = 403 (authenticated, but role/policy check failed).
            int status = authorizeResult.Challenged ? StatusCodes.Status401Unauthorized
                                                    : StatusCodes.Status403Forbidden;

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var role = context.User.FindFirstValue(ClaimTypes.Role);
            var identity = role is null ? userId : $"{userId} ({role})";
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            logger.LogWarning(
                "Authorization denied ({Status}) on {Path} for user {Identity} from {IP}",
                status, context.Request.Path, identity, ip);
        }

        await _inner.HandleAsync(next, context, policy, authorizeResult);
    }
}

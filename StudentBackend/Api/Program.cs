using StudentApi.Backend.Extensions;
using Scalar.AspNetCore;
using StudentBackend.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Composition root: each subsystem lives in its own extension under Extensions/.
builder.Services.AddInfrastructure(builder.Configuration);
builder.AddAuditLogging();                // Serilog: console + app/warnings/errors files
builder.AddJwtAuthentication();           // JWT bearer + StudentOwnerOrAdmin policy
builder.Services.AddApiRateLimiting();    // AuthLimiter / ApiLimiter / HeavyLimiter
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApiDocs();        // OpenAPI doc with Bearer security scheme

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
// The rate limiter must run AFTER authentication: its per-user policies
// partition by the authenticated identity (httpContext.User), which is only
// populated once authentication has run.
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");

app.Run();

using Serilog;
using Serilog.Events;

namespace StudentApi.Backend.Extensions;

/// <summary>
/// Security-audit logging: Console + one general daily file + filtered
/// quick-triage copies (warnings / errors). Deliberately synchronous - no
/// Async wrapper: audit events must be on disk before the request ends.
/// </summary>
public static class SerilogExtensions
{
    public static void AddAuditLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, config) => config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .WriteTo.Console()
            .WriteTo.File("logs/app-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true,
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            // Sub-streams: filtered quick-triage COPIES. Every line still lands in
            // app-*.txt first, so the chronological story stays in one place - these
            // are views over it, not replacements.
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                .WriteTo.File("logs/warnings-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    shared: true,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))
            // Errors + Fatal: technical failures, kept longer than the noise.
            .WriteTo.Logger(lc => lc
                .MinimumLevel.Error()
                .WriteTo.File("logs/errors-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 60,
                    shared: true,
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")));
    }
}

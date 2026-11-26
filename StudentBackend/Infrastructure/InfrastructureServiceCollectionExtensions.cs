using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StudentBackend.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StudentsDbContext>(options =>
            options.UseStudentsDBNpgsql(configuration));

        services.AddIdentityCore<AppUser>(options =>
        {
            options.User.RequireUniqueEmail = true;

            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<StudentsDbContext>()
        .AddSignInManager();

        return services;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace StudentBackend.Infrastructure;

public static class NpgsqlDbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseStudentsDBNpgsql(
        this DbContextOptionsBuilder options,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StudentsDb")
            ?? throw new InvalidOperationException(
                "Connection string 'StudentsDb' was not found.");

        return options.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
    }
}

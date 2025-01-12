using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Btms.Backend.Data.Healthcheck;

/// <summary>
/// Extension methods to configure <see cref="MongoDbHealthCheck"/>.
/// </summary>
public static class MongoDbHealthCheckBuilderExtensions
{
    private const string Name = "mongodb";

    public static IHealthChecksBuilder AddMongoDb(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? Name,
            sp =>
            {
                var options = sp.GetService<IOptions<MongoDbOptions>>();
                return new MongoDbHealthCheck(options?.Value.DatabaseUri!, options?.Value.DatabaseName!);
            },
            failureStatus,
            tags,
            timeout));
    }
}
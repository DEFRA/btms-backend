using Cdms.Backend.Data.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Cdms.Backend.Data.Healthcheck;

/// <summary>
/// Extension methods to configure <see cref="MongoDbHealthCheck"/>.
/// </summary>
public static class MongoDbHealthCheckBuilderExtensions
{
    private const string NAME = "mongodb";

    public static IHealthChecksBuilder AddMongoDb(
        this IHealthChecksBuilder builder,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                var options = sp.GetService<IOptions<MongoDbOptions>>();
                return new MongoDbHealthCheck(options.Value.DatabaseUri, options.Value.DatabaseName);
            },
            failureStatus,
            tags,
            timeout));
    }
}
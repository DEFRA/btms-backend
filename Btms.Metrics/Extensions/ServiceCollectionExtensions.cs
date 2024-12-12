using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Btms.Metrics.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBtmsMetrics(this IServiceCollection services)
    {
        services.TryAddSingleton<ConsumerMetrics>();
        services.TryAddSingleton<SyncMetrics>();
        services.TryAddSingleton<InMemoryQueueMetrics>();
        services.TryAddSingleton<LinkingMetrics>();

        return services;
    }
}
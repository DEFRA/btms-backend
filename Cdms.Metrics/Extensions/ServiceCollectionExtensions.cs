using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cdms.Metrics.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCdmsMetrics(this IServiceCollection services)
        {
            services.TryAddSingleton<ConsumerMetrics>();
            services.TryAddSingleton<SyncMetrics>();
            services.TryAddSingleton<InMemoryQueueMetrics>();
            services.TryAddSingleton<LinkingMetrics>();

            return services;
        }
    }
}
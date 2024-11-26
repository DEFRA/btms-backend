using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Analytics.Extensions;

public static class AnalyticsExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IMatchingAggregationService, MatchingAggregationService>();
        return services;
    }
}
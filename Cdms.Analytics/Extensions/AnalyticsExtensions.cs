using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Analytics.Extensions;

public static class AnalyticsExtensions
{   
    public static IServiceCollection AddServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IMatchingAggregationService, MatchingAggregationService>();
        return services;
    }
}
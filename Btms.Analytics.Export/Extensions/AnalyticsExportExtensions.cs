using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Analytics.Export.Extensions;

public static class AnalyticsExportExtensions
{
    public static IServiceCollection AddAnalyticsExportServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<MovementExportService, MovementExportService>();
        
        return services;
    }
}
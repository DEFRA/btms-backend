using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator.Helpers;

public static class BuilderExtensions
{   
    public static IServiceCollection ConfigureTestGenerationServices(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddSingleton<ChedASimpleMatchScenarioGenerator>();
        services.AddSingleton<ChedAManyCommoditiesScenarioGenerator>();
        services.AddSingleton<ChedPSimpleMatchScenarioGenerator>();
        services.AddSingleton<ChedANoMatchScenarioGenerator>();
        services.AddSingleton<CrNoMatchScenarioGenerator>();
                
        var blobOptionsValidatorDescriptor = services.Where(d => 
            d.ServiceType == typeof(IValidateOptions<BlobServiceOptions>));

        foreach (var serviceDescriptor in blobOptionsValidatorDescriptor.ToList())
        {
            services.Remove(serviceDescriptor);
        }
        
        return services;
    }
}
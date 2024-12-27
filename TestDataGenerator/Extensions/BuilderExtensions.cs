using Btms.BlobService;
using Btms.BlobService.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestDataGenerator.Config;
using TestDataGenerator.Helpers;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator.Extensions;

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
        services.AddSingleton<ChedPMultiStepScenarioGenerator>();
                
        var blobOptionsValidatorDescriptor = services.Where(d => 
            d.ServiceType == typeof(IValidateOptions<BlobServiceOptions>));

        foreach (var serviceDescriptor in blobOptionsValidatorDescriptor.ToList())
        {
            services.Remove(serviceDescriptor);
        }
        
        return services;
    }

    private static (IConfigurationRoot, GeneratorConfig) GetConfig(string cachePath)
    {   
        // Any defaults for the test generation can be added here
        var configurationValues = new Dictionary<string, string>
        {
            { "BlobServiceOptions:CachePath", cachePath },
            { "BlobServiceOptions:CacheReadEnabled", "true" },
            { "BlobServiceOptions:CacheWriteEnabled", "true" }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(configurationValues!)
            .AddIniFile("Properties/local.env", true)
            .Build();
        
        var generatorConfig = new GeneratorConfig(configuration);

        return (configuration, generatorConfig);
    }

    private static void ConfigureAppConfiguration(this IConfigurationBuilder builder, IConfigurationRoot configuration)
    {
        builder.Sources.Clear();
        builder.AddConfiguration(configuration);
    }

    private static void ConfigureServices(this IServiceCollection services, IConfigurationRoot configuration, GeneratorConfig generatorConfig)
    {
        services.AddHttpClient();

        services.AddSingleton<GeneratorConfig>(_ => generatorConfig);
        services.AddBlobStorage(configuration);

        services.AddTransient<Generator>();
                
        services.ConfigureTestGenerationServices();
    }
    
    // TODO : integration tests currently uses IWebHostBuilder, so this allowed us to set that up
    // have switched to a seperate host, which uses IHostBuilder  
    // public static IWebHostBuilder ConfigureTestDataGenerator(this IWebHostBuilder hostBuilder,
    //     string cachePath = "../../../.test-data-generator")
    // {
    //     var (configuration, generatorConfig) = GetConfig(cachePath);
    //
    //     hostBuilder
    //         .ConfigureAppConfiguration(builder => builder.ConfigureAppConfiguration(configuration))
    //         .ConfigureServices((_, services) => services.ConfigureServices(configuration, generatorConfig));
    //     //TODO - why doesn't AddLogging work?... 
    //     // .AddLogging();
    //
    //     return hostBuilder;
    // }
    
    public static IHostBuilder ConfigureTestDataGenerator(this IHostBuilder hostBuilder,
        string cachePath = "../../../.test-data-generator")
    {
        var (configuration, generatorConfig) = GetConfig(cachePath);
        
        hostBuilder
            .ConfigureAppConfiguration(builder => builder.ConfigureAppConfiguration(configuration))
            .ConfigureServices((_, services) => services.ConfigureServices(configuration, generatorConfig))
            .AddLogging();

        return hostBuilder;
    }
}
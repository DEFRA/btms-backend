using Btms.BlobService;
using Btms.BlobService.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TestDataGenerator.Config;

namespace TestDataGenerator.Extensions;

public static class BuilderExtensions
{
    public static object[] BuildAll(this IBaseBuilder[] builders)
    {
        var messages = builders
            .Select<IBaseBuilder,object>(b =>
            {
                switch (b)
                {
                    // TODO - if IBaseBuilder had the Build method this wouldn't
                    // be necessary
                    case ClearanceRequestBuilder builder:
                        return builder.Build();
                        
                    case ImportNotificationBuilder builder:
                        return builder.Build();
                    
                    case DecisionBuilder builder:
                        return builder.Build();
                    
                    case FinalisationBuilder builder:
                        return builder.Build();
                    
                    default:
                        throw new InvalidDataException($"Unexpected type {b.GetType().Name}");
                }
                
            });
        
        return messages
            .OrderBy(m => m.CreatedDate())
            .ToArray();
    }

    /// <summary>
    /// // Find all scenario generators
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Type> GetAllScenarios()
    {
        Type scenarioService = typeof(ScenarioGenerator);

        return AppDomain
            .CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(
                p => scenarioService.IsAssignableFrom(p)
                     && !p.IsAbstract
                     && p != scenarioService);
    }

    public static ServiceProvider GetDefaultServiceProvider()
    {
        var (configuration, _) = BuilderExtensions.GetConfig("Scenarios/Samples");
        
        return new ServiceCollection()
            .AddBlobStorage(configuration)
            .AddSingleton<CachingBlobService>()
            .ConfigureTestGenerationServices()
            .BuildServiceProvider();
    }
    
    public static IServiceCollection ConfigureTestGenerationServices(this IServiceCollection services)
    {
        services.AddHttpClient();
        
        foreach (var type in GetAllScenarios())
        {
            services.AddSingleton(type);
        }

        var blobOptionsValidatorDescriptor = services.Where(d => 
            d.ServiceType == typeof(IValidateOptions<BlobServiceOptions>));

        foreach (var serviceDescriptor in blobOptionsValidatorDescriptor.ToList())
        {
            services.Remove(serviceDescriptor);
        }
        
        
        return services;
    }

    public static (IConfigurationRoot, GeneratorConfig) GetConfig(string cachePath = "../../../.test-data-generator")
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
            .ConfigureServices((_, services) =>
                {
                    services.ConfigureServices(configuration, generatorConfig);
                    // services.AddSingleton<IBlobService, CachingBlobService>();
                    services.AddSingleton<CachingBlobService>();
                }
            )
            .AddLogging();

        return hostBuilder;
    }
}
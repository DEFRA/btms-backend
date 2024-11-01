using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Services;

namespace TestDataGenerator;

internal class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddIniFile("Properties/local.env", true)
            .Build();

        var generatorConfig = new GeneratorConfig(configuration);
        
        var builder = Host.CreateDefaultBuilder(args)
            // .ConfigureAppConfiguration(c =>
            // {
            //     c.AddEnvironmentVariables("CDP");
            //     c.AddEnvironmentVariables();
            //     c.AddIniFile("Properties/local.env", true);
            // })
            .ConfigureAppConfiguration(builder =>
            {
                builder.Sources.Clear();
                builder.AddConfiguration(configuration);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();

                services.AddSingleton<GeneratorConfig, GeneratorConfig>(s => generatorConfig);
                services.AddSingleton<ChedASimpleMatchScenarioGenerator, ChedASimpleMatchScenarioGenerator>();
                services.AddSingleton<ChedAManyCommoditiesScenarioGenerator, ChedAManyCommoditiesScenarioGenerator>();
                if (generatorConfig.StorageService == StorageService.Local)
                {
                    services.AddSingleton<IBlobService, LocalBlobService>();   
                }
                else
                {
                    services.AddSingleton<IBlobService, BlobService>();
                }
                
                services.AddTransient<Generator>();
            })
            .AddLogging();
       
            
        Console.WriteLine("Welcome to test data generator.");
        
        var app = builder.Build();
        var config = app.Services.GetRequiredService<GeneratorConfig>();
        var generator = app.Services.GetRequiredService<Generator>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var chedASimpleMatch = app.Services.GetRequiredService<ChedASimpleMatchScenarioGenerator>();
        var chedAManyCommodities = app.Services.GetRequiredService<ChedAManyCommoditiesScenarioGenerator>();
        
        var scenarios = new[]
        {
            // new { scenario = "ChedASimpleMatch", count = 10, days = 1, generator = (ScenarioGenerator)chedASimpleMatch },
            new { scenario = "ChedAManyCommodities", count = 1, days = 5, generator = (ScenarioGenerator)chedAManyCommodities }
        };
        
        logger.LogInformation($"{scenarios.Length} scenario(s) configured");
        
        logger.LogInformation("Clearing down storage path");
        await generator.Cleardown();
        logger.LogInformation("Cleared down");
        
        foreach (var s in scenarios)
        {
            await generator.Generate(s.count, s.days, s.generator);
        }
        
        logger.LogInformation("Done");
    }
}
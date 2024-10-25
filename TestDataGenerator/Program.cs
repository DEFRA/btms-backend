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
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(c =>
            {
                c.AddEnvironmentVariables("CDP");
                c.AddEnvironmentVariables();
                c.AddIniFile("Properties/local.env", true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();

                services.AddSingleton<GeneratorConfig, GeneratorConfig>();
                services.AddSingleton<SimpleChedAMatchScenarioGenerator, SimpleChedAMatchScenarioGenerator>();
                services.AddSingleton<IBlobService, BlobService>();
                services.AddTransient<Generator>();
            })
            .AddLogging();
       
            
        Console.WriteLine("Welcome to test data generator.");
        
        var app = builder.Build();
        var generator = app.Services.GetRequiredService<Generator>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var simpleChedAMatch = app.Services.GetRequiredService<SimpleChedAMatchScenarioGenerator>();
        
        var scenarios = new[] { new {scenario = "SimpleChedAMatch", count = 10, generator = simpleChedAMatch } };
        
        logger.LogInformation($"{scenarios.Length} scenario(s) configured");
        
        logger.LogInformation("Clearimg down storage path");
        await generator.Cleardown();
        logger.LogInformation("Cleared down");
        
        foreach (var s in scenarios)
        {
            await generator.Generate(s.count, s.generator);
        }
        
        logger.LogInformation("Done");
    }
}
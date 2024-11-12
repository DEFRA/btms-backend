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

        var datasets = new[]
        {
            new
            {
                dataset = "LoadTest",
                rootPath = "GENERATED-LOADTEST-90Dx10k",
                scenarios = new[]
                {
                    new
                    {
                        scenario = "ChedASimpleMatch",
                        count = 10000,
                        days = 90,
                        generator = (ScenarioGenerator)chedASimpleMatch
                    },
                    new
                    {
                        scenario = "ChedAManyCommodities",
                        count = 100,
                        days = 14,
                        generator = (ScenarioGenerator)chedAManyCommodities
                    }
                }
            },
            new
            {
                dataset = "PHA",
                rootPath = "GENERATED-PHA",
                scenarios = new[]
                {
                    new
                    {
                        scenario = "ChedASimpleMatch",
                        count = 10,
                        days = 30,
                        generator = (ScenarioGenerator)chedASimpleMatch
                    },
                    new
                    {
                        scenario = "ChedAManyCommodities",
                        count = 10,
                        days = 30,
                        generator = (ScenarioGenerator)chedAManyCommodities
                    }
                }
            }
        };

        logger.LogInformation("{datasetsCount} dataset(s) configured", datasets.Length);

        // Allows us to filter the sets and scenarios we want to run at any given time
        // Could be fed by CLI for example
        var setsToRun = datasets
            .Where(d => d.dataset == "LoadTest")
            .Select(d => new {
                scenarios = d.scenarios
                    .Where(s =>
                        s.scenario == "ChedASimpleMatch"
                    ),
                dataset = d.dataset,
                rootPath = d.rootPath
            });

        foreach (var dataset in setsToRun)
        {
            logger.LogInformation("{scenariosCount} scenario(s) configured", dataset.scenarios.Count());

            logger.LogInformation("Clearing down storage path");
            await generator.Cleardown(dataset.rootPath);
            logger.LogInformation("Cleared down");

            foreach (var s in dataset.scenarios)
            {
                await generator.Generate(s.count, s.days, s.generator, dataset.rootPath);
            }

            logger.LogInformation("{dataset} Done", dataset.dataset);
        }

        logger.LogInformation("Done");
    }
}
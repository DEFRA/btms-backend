using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using Btms.BlobService.Extensions;
using Microsoft.Extensions.Options;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

class Program
{
    private static async Task Main(string[] args)
    {
        
        // Any defaults for the test generation can be added here
        var configurationValues = new Dictionary<string, string>
        {
            { "BlobServiceOptions:CachePath", "../../../.test-data-generator" },
            { "BlobServiceOptions:CacheReadEnabled", "true" },
            { "BlobServiceOptions:CacheWriteEnabled", "true" }
        };
        
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddInMemoryCollection(configurationValues!)
            .AddIniFile("Properties/local.env", true)
            .Build();
        
        var generatorConfig = new GeneratorConfig(configuration);

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder =>
            {
                builder.Sources.Clear();
                builder.AddConfiguration(configuration);
            })
            .ConfigureServices((_, services) =>
            {
                services.AddHttpClient();

                services.AddSingleton<GeneratorConfig>(_ => generatorConfig);
                services.AddBlobStorage(configuration);

                services.AddTransient<Generator>();
                
                services.ConfigureTestGenerationServices();
                
            })
            .AddLogging();

        var app = builder.Build();
        var generator = app.Services.GetRequiredService<Generator>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var datasets = new[]
        {
            new
            {
                Dataset = "EndToEnd-IBM",
                RootPath = "GENERATED-ENDTOEND-IBM",
                Scenarios = new[] { app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(1, 1) }
            },
            new
            {
                Dataset = "LoadTest-One",
                RootPath = "GENERATED-LOADTEST-ONE",
                Scenarios = new[] { app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(1, 3) }
            },
            new
            {
                Dataset = "LoadTest-Basic",
                RootPath = "GENERATED-LOADTEST-BASIC",
                Scenarios = new[]
                {
                    app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(3, 7),
                    app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(3, 7),
                    app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(3, 7)
                }
            },
            new
            {
                Dataset = "LoadTest",
                RootPath = "GENERATED-LOADTEST",
                Scenarios = new[]
                {
                    app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(100, 90),
                    app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(100, 90),
                    app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(100, 90)
                }
            },
            new
            {
                Dataset = "LoadTest-Condensed",
                RootPath = "GENERATED-CONDENSED",
                Scenarios = new[]
                {
                    app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(5, 7),
                    app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(5, 7),
                    app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(15, 7)
                }
            },
            new
            {
                Dataset = "LoadTest-90Dx10k",
                RootPath = "GENERATED-LOADTEST-90Dx10k",
                Scenarios =
                    new[]
                    {
                        app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(5000, 90),
                        app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(100, 90),
                        app.CreateScenarioConfig<ChedPSimpleMatchScenarioGenerator>(4900, 90)
                    }
            },
            new
            {
                Dataset = "PHA",
                RootPath = "GENERATED-PHA",
                Scenarios = new[]
                {
                    app.CreateScenarioConfig<ChedASimpleMatchScenarioGenerator>(10, 30),
                    app.CreateScenarioConfig<ChedAManyCommoditiesScenarioGenerator>(10, 30)
                }
            }
        };

        logger.LogInformation("{DatasetsCount} dataset(s) configured", datasets.Length);

        var ds = args.Length > 0 ? args[0].Split(",") : ["LoadTest-One"];
        var setsToRun = datasets
            .Where(d => ds.Contains(d.Dataset));

        var scenario = 1;

        foreach (var dataset in setsToRun)
        {
            logger.LogInformation("{scenariosCount} scenario(s) configured", dataset.Scenarios.Count());

            await generator.Cleardown(dataset.RootPath);

            foreach (var s in dataset.Scenarios)
            {
                await generator.Generate(scenario, s, dataset.RootPath);
                scenario++;
            }

            logger.LogInformation("{dataset} Done", dataset.Dataset);
        }

        logger.LogInformation("Done");
    }
}
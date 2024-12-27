using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Config;
using TestDataGenerator.Scenarios;
using Btms.BlobService.Extensions;
using TestDataGenerator.Extensions;
using TestDataGenerator.Helpers;

namespace TestDataGenerator;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureTestDataGenerator();

        var app = builder.Build();
        var generator = app.Services.GetRequiredService<Generator>();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        var datasets = Datasets.GetDatasets(app);

        logger.LogInformation("{DatasetsCount} dataset(s) configured", datasets.Length);

        var ds = args.Length > 0 ? args[0].Split(",") : ["One"];
        var setsToRun = datasets
            .Where(d => ds.Contains(d.Name));

        var scenario = 1;

        foreach (var dataset in setsToRun)
        {
            logger.LogInformation("{ScenariosCount} scenario(s) configured", dataset.Scenarios.Count());

            await generator.Cleardown(dataset.RootPath);

            foreach (var s in dataset.Scenarios)
            {
                await generator.Generate(scenario, s, dataset.RootPath);
                scenario++;
            }

            logger.LogInformation("{Dataset} Done", dataset.Name);
        }

        logger.LogInformation("Done");
    }
}
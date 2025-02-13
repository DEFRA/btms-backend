using Btms.Analytics;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDataGenerator;
using TestDataGenerator.Config;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Btms.Backend.IntegrationTests.Extensions;

public static class DatasetExtensions
{
    public static async Task<List<(ScenarioGenerator generator, int scenario, int dateOffset, int count, object message)>> Generate(this Dataset dataset, IHost app, ITestOutputHelper logger)
    {
        var output = new List<(ScenarioGenerator generator, int scenario, int dateOffset, int count, object message)>();
        var generator = app.Services.GetRequiredService<Generator>();

        logger.WriteLine("{0} scenario(s) configured", dataset.Scenarios.Count());

        var scenario = 1;

        await generator.Cleardown(dataset.RootPath);

        foreach (var s in dataset.Scenarios)
        {
            var generatorResult = await generator.Generate(scenario, s, dataset.RootPath);
            var results = generatorResult
                .SelectMany(gr => gr.result
                    .Select(r => (gr.generator, gr.scenario, gr.dateOffset, gr.count, r)));

            // generatorResult.SelectMany(r => r.result.GetEnumerator(). (r.generator, r.scenario, r.dateOffset, r.count, ))
            output.AddRange(results);
            scenario++;
        }

        logger.WriteLine("{0} Done", dataset.Name);

        return output;
    }

}
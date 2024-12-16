using Btms.Consumers;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using SlimMessageBus.Host;
using TestDataGenerator;
using TestDataGenerator.Scenarios;

namespace Btms.Analytics.Tests.Helpers;

public static class TestDataGeneratorHelpers
{
    private static int _scenarioIndex;
    
    public static async Task<IHost> PushToConsumers(this IHost app, ScenarioConfig scenario)
    {
        var generatorResults = app.Generate(scenario);
        _scenarioIndex++;
        
        app.Services.GetRequiredService<ILogger<ScenarioGenerator>>();
        
        foreach (var generatorResult in generatorResults)
        {
            foreach (var cr in generatorResult.ClearanceRequests)
            {
                var scope = app.Services.CreateScope();
                var consumer = (AlvsClearanceRequestConsumer)scope.ServiceProvider.GetRequiredService<IConsumer<AlvsClearanceRequest>>();
        
                consumer.Context = new ConsumerContext
                {
                    Headers = new Dictionary<string, object> { { "messageId", cr.Header!.EntryReference! } }
                };
            
                await consumer.OnHandle(cr);
            }
        
            foreach (var n in generatorResult.ImportNotifications)
            {
                var scope = app.Services.CreateScope();
                var consumer = (NotificationConsumer)scope.ServiceProvider.GetRequiredService<IConsumer<ImportNotification>>();
        
                consumer.Context = new ConsumerContext
                {
                    Headers = new Dictionary<string, object> { { "messageId", n.ReferenceNumber! } }
                };
            
                await consumer.OnHandle(n);
            }
        }

        return app;
    } 
    
    private static ScenarioGenerator.GeneratorResult[] Generate(this IHost app, ScenarioConfig scenario)
    {
        var logger = app.Services.GetRequiredService<ILogger<ScenarioGenerator>>();
        var days = scenario.CreationDateRange;
        var count = scenario.Count;
        var generator = scenario.Generator;
        
        logger.LogInformation("Generating {Count}x{Days} {@Generator}", count, days, generator);
        var results = new List<ScenarioGenerator.GeneratorResult>();
        
        for (var d = -days + 1; d <= 0; d++)
        {
            logger.LogInformation("Generating day {D}", d);
            var entryDate = DateTime.Today.AddDays(d);

            for (var i = 0; i < count; i++)
            {
                logger.LogInformation("Generating item {I}", i);

                results.Add(generator.Generate(_scenarioIndex, i, entryDate, scenario));
            }
        }

        return results.ToArray();
    }
}
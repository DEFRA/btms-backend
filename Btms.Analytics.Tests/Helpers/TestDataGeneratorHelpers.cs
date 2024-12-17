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
    private static int scenarioIndex;
    
    public static async Task<IHost> PushToConsumers(this IHost app, ScenarioConfig scenario)
    {
        var generatorResults = app.Generate(scenario);
        scenarioIndex++;
        
        app.Services.GetRequiredService<ILogger<ScenarioGenerator>>();
        
        foreach (var generatorResult in generatorResults)
        {
            foreach (var message in generatorResult)
            {
                var scope = app.Services.CreateScope();
                
                switch (message)
                {
                    case null:
                        throw new ArgumentNullException();
                    
                    case ImportNotification n:
                        
                        var notificationConsumer = (NotificationConsumer)scope
                            .ServiceProvider
                            .GetRequiredService<IConsumer<ImportNotification>>();
        
                        notificationConsumer.Context = new ConsumerContext
                        {
                            Headers = new Dictionary<string, object> { { "messageId", n.ReferenceNumber! } }
                        };
            
                        await notificationConsumer.OnHandle(n);
                        break;
                    
                    case AlvsClearanceRequest cr:
                        
                        var alvsConsumer = (AlvsClearanceRequestConsumer)scope
                            .ServiceProvider
                            .GetRequiredService<IConsumer<AlvsClearanceRequest>>();
                        
                        alvsConsumer.Context = new ConsumerContext
                        {
                            Headers = new Dictionary<string, object> { { "messageId", cr.Header!.EntryReference! } }
                        };
            
                        await alvsConsumer.OnHandle(cr);
                        break;
                        
                    default:
                        throw new ArgumentException($"Unexpected type {message.GetType().Name}");
                }
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

                results.Add(generator.Generate(scenarioIndex, i, entryDate, scenario));
            }
        }

        return results.ToArray();
    }
}
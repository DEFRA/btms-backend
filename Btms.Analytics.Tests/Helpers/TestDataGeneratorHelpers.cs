using Btms.Common.Extensions;
using Btms.Consumers;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Btms.SyncJob.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SlimMessageBus;
using SlimMessageBus.Host;
using TestDataGenerator;
using TestDataGenerator.Scenarios;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Analytics.Tests.Helpers;

public static class TestDataGeneratorHelpers
{
    private static int scenarioIndex;
    
    public static async Task<IHost> PushToConsumers(this IHost app, ILogger logger, ScenarioConfig scenario)
    {
        var generatorResults = app.Generate(logger, scenario);
        scenarioIndex++;
        
        // var logger = app.Services.GetRequiredService<ILogger<ScenarioGenerator>>();
        var bus = app.Services.GetRequiredService<IPublishBus>();
        
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
                        logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
                        break;
                    
                    case Decision d:
                        
                        var decisionConsumer = (DecisionsConsumer)scope
                            .ServiceProvider
                            .GetRequiredService<IConsumer<Decision>>();

                        decisionConsumer.Context = new ConsumerContext
                        {
                            Headers = new Dictionary<string, object> { { "messageId", d.Header!.EntryReference! } }
                        };
                        
                        await decisionConsumer.OnHandle(d);
                        logger.LogInformation("Sent decision {0} to consumer", d.Header!.EntryReference!);
                        break;
                    
                    case AlvsClearanceRequest cr:
                        
                        var crConsumer = (AlvsClearanceRequestConsumer)scope
                            .ServiceProvider
                            .GetRequiredService<IConsumer<AlvsClearanceRequest>>();
                        
                        crConsumer.Context = new ConsumerContext
                        {
                            Headers = new Dictionary<string, object> { { "messageId", cr.Header!.EntryReference! } }
                        };
                        
                        await crConsumer.OnHandle(cr);
                        logger.LogInformation("Semt cr {0} to consumer", cr.Header!.EntryReference!);
                        break;
                        
                    default:
                        throw new ArgumentException($"Unexpected type {message.GetType().Name}");
                }
            }
        }

        return app;
    } 
    
    private static ScenarioGenerator.GeneratorResult[] Generate(this IHost app, ILogger logger, ScenarioConfig scenario)
    {
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
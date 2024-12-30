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
    
    public static async Task<IServiceProvider> GeneratorPushToConsumers(this IServiceProvider sp, ILogger logger, ScenarioConfig scenario)
    {
        var generatorResults = scenario.Generate(logger, scenarioIndex);
        scenarioIndex++;
        
        // var logger = app.Services.GetRequiredService<ILogger<ScenarioGenerator>>();
        // var bus = sp.GetRequiredService<IPublishBus>();
        
        foreach (var generatorResult in generatorResults)
        {
            foreach (var message in generatorResult)
            {
                var scope = sp.CreateScope();
                
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
                        // This sleep is to allow the system to settle, and have made any links & decisions
                        // Before sending in a decision and causing a concurrency issue
                        // Ideally we want to switch to pushing to the bus, rather than directly to the consumer
                        // so we get the concurrency protection.
                        
                        Thread.Sleep(1000);
                        
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

        return sp;
    } 
    
    // public static ScenarioGenerator.GeneratorResult[] Generate(this IServiceProvider sp, scenarioIndex, ILogger logger, ScenarioConfig scenario)
    // {
    //     var days = scenario.CreationDateRange;
    //     var count = scenario.Count;
    //     var generator = scenario.Generator;
    //     
    //     logger.LogInformation("Generating {Count}x{Days} {@Generator}", count, days, generator);
    //     var results = new List<ScenarioGenerator.GeneratorResult>();
    //     
    //     for (var d = -days + 1; d <= 0; d++)
    //     {
    //         logger.LogInformation("Generating day {D}", d);
    //         var entryDate = DateTime.Today.AddDays(d);
    //
    //         for (var i = 0; i < count; i++)
    //         {
    //             logger.LogInformation("Generating item {I}", i);
    //
    //             results.Add(generator.Generate(scenarioIndex, i, entryDate, scenario));
    //         }
    //     }
    //
    //     return results.ToArray();
    // }
}
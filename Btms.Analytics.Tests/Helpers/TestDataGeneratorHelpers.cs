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
                var bus = scope.ServiceProvider.GetRequiredService<IPublishBus>();
                var headers = new Dictionary<string, object>();

                switch (message)
                {
                    case null:
                        throw new ArgumentNullException();

                    case ImportNotification n:
                        headers.Add("messageId", n.ReferenceNumber!);
                        await bus.Publish(n, "NOTIFICATIONS", headers);
                        logger.LogInformation("Sent notification {0} to consumer", n.ReferenceNumber!);
                        break;

                    case Decision d:
                        headers.Add("messageId", d.Header!.EntryReference!);
                        await bus.Publish(d, "DECISIONS", headers);
                        logger.LogInformation("Sent decision {0} to consumer", d.Header!.EntryReference!);
                        break;

                    case AlvsClearanceRequest cr:
                        headers.Add("messageId", cr.Header!.EntryReference!);
                        await bus.Publish(cr, "CLEARANCEREQUESTS", headers);
                        logger.LogInformation("Sent cr {0} to consumer", cr.Header!.EntryReference!);
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
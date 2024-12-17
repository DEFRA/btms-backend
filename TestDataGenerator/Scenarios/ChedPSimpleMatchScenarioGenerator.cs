using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ChedPSimpleMatchScenarioGenerator(ILogger<ChedPSimpleMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = GetNotificationBuilder("chedp-one-commodity")
            .WithEntryDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithCommodity("1604142800", "Skipjack Tuna", 300)
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithEntryDate(entryDate)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithItem("N853", "16041421", "Tuna ROW CHEDP", 900)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecision = GetDecisionBuilder("decision-one-item")
            .WithEntryDate(entryDate.AddHours(1))
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithItemAndCheck(1, "H222", "H01")
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, notification, alvsDecision]);
    }
}
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios;

public class ChedAManyCommoditiesScenarioGenerator(ILogger<ChedAManyCommoditiesScenarioGenerator> logger)
    : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = BuilderHelpers.GetNotificationBuilder("cheda-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .WithRandomCommodities(10, 100)
            .WithInspectionStatus()
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}",
            notification.ReferenceNumber);

        var clearanceRequest = BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest, notification]);

    }
}
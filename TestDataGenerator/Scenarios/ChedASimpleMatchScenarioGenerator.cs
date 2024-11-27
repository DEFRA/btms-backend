using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ChedASimpleMatchScenarioGenerator(ILogger<ChedASimpleMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        // TODO : get a good 'pair' of notification and cr as the source templates

        var notification = GetNotificationBuilder("cheda-one-commodity")
            .WithEntryDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .ValidateAndBuild()!;

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithEntryDate(entryDate)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult { ClearanceRequests = [clearanceRequest], ImportNotifications = [notification] };
    }
}
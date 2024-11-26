using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ChedPSimpleMatchScenarioGenerator(ILogger<ChedPSimpleMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate)
    {
        var notification = GetNotificationBuilder("chedp-one-commodity")
            .WithEntryDate(entryDate)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithCommodity("1604142800", "Skipjack Tuna", 300)
            .ValidateAndBuild();

        logger.LogInformation("Created {@Notification}, {NotificationReferenceNumber}", notification,
            notification.ReferenceNumber);

        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithEntryDate(entryDate)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithItem("N853", "16041421", "Tuna ROW CHEDP", 900)
            .ValidateAndBuild();

        logger.LogInformation("Created {@ClearanceRequest}, {EntryReference}", clearanceRequest,
            clearanceRequest.Header!.EntryReference);

        return new GeneratorResult { ClearanceRequests = [clearanceRequest], ImportNotifications = [notification] };
    }
}
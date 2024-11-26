using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ChedANoMatchScenarioGenerator(ILogger<ChedANoMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate)
    {
        var notification = GetNotificationBuilder("cheda-one-commodity")
            .WithEntryDate(entryDate)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .ValidateAndBuild()!;

        logger.LogInformation("Created no match notification {@Notification}, {NotificationReferenceNumber}", notification,
            notification.ReferenceNumber);

        return new GeneratorResult { ClearanceRequests = [], ImportNotifications = [notification] };
    }
}
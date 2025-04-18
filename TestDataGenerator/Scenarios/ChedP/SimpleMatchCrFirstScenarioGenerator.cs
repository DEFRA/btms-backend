using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios.ChedP;

public class SimpleMatchCrFirstScenarioGenerator(ILogger<SimpleMatchCrFirstScenarioGenerator> logger)
    : SimpleMatchScenarioGenerator(logger);

public class SimpleMatchNotificationFirstScenarioGenerator(ILogger<SimpleMatchNotificationFirstScenarioGenerator> logger)
    : SimpleMatchScenarioGenerator(logger, false);

public abstract class SimpleMatchScenarioGenerator(ILogger logger, bool crFirst = true) : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = BuilderHelpers.GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithSimpleCommodity("1604142800", "Skipjack Tuna", 300)
            .WithInspectionStatus(InspectionRequiredEnum.NotRequired) //NB, the examples in the redacted data are title case, but code is uppercase CDMS-210
            .WithIuuOption(ControlAuthorityIuuOptionEnum.Iuuok)
            .WithVersionNumber()
            .ValidateAndBuild();

        var clearanceRequest = BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithTunaItem()
            .ValidateAndBuild();

        Logger.LogInformation("Created Notification {NotificationReference}, Clearance Request {ClearanceRequest}",
            notification.ReferenceNumber, clearanceRequest.Header!.EntryReference);

        var alvsDecision = BuilderHelpers.GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithItemAndCheck(1, "H222", "H01")
            .WithItemAndCheck(1, "H224", "C07")
            .ValidateAndBuild();

        Logger.LogInformation("Created Decision {EntryReference}", alvsDecision.Header!.EntryReference);

        return new GeneratorResult(crFirst ? [clearanceRequest, notification, alvsDecision] : [notification, clearanceRequest, alvsDecision]);
    }
}
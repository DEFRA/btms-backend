using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using TestDataGenerator.Helpers;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator.Extensions;

public static class IScenarioGeneratorExtensions
{
    public static ImportNotificationBuilder<ImportNotification> GetSimpleNotification(this ScenarioGenerator generator, int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        return BuilderHelpers.GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithSimpleCommodity("1604142800", "Skipjack Tuna", 300)
            .WithIuuOption(ControlAuthorityIuuOptionEnum.Iuuok)
            .WithInspectionStatus(InspectionRequiredEnum.NotRequired)
            .WithVersionNumber();
    }

    public static ClearanceRequestBuilder<AlvsClearanceRequest> GetSimpleClearanceRequest(this ScenarioGenerator generator, int scenario, int item, DateTime entryDate, ScenarioConfig config, ImportNotification notification)
    {
        return BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithTunaItem();
    }

    public static DecisionBuilder<Btms.Types.Alvs.Decision> GetSimpleDecision(this ScenarioGenerator generator, int scenario, int item, DateTime entryDate, ScenarioConfig config, ImportNotification notification, AlvsClearanceRequest clearanceRequest)
    {
        return BuilderHelpers.GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithTunaChecks();
    }
}
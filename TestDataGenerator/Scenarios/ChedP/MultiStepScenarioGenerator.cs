using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios.ChedP;

public class MultiStepScenarioGenerator(ILogger<MultiStepScenarioGenerator> logger) : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notificationBuilder = BuilderHelpers.GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate.AddDays(-1))
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithNoCommodities()
            .WithInspectionStatus(InspectionRequiredEnum.NotRequired) //NB, the examples in the redacted data are title case, but code is uppercase CDMS-210
            .WithIuuOption(ControlAuthorityIuuOptionEnum.Iuuok)
            .WithVersionNumber();

        var notification = notificationBuilder
            .ValidateAndBuild();

        var clearanceRequest = BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithTunaItem()
            .ValidateAndBuild();

        var alvsDecision = BuilderHelpers.GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithTunaChecks()
            .ValidateAndBuild();

        logger.LogInformation("Created Notification {NotificationReference}, Clearance Request {ClearanceRequest}, Decision {EntryReference}",
            notification.ReferenceNumber, clearanceRequest.Header!.EntryReference, alvsDecision.Header!.EntryReference);

        var uniqueCommodityId = Guid.NewGuid();

        var updatedNotification = notificationBuilder
            .Clone()
            .WithCreationDate(entryDate)
            .WithSimpleCommodity("1604142800", "Skipjack Tuna", 300, uniqueCommodityId)
            .WithRiskAssesment(uniqueCommodityId, CommodityRiskResultRiskDecisionEnum.Notrequired)
            .WithVersionNumber(2)
            .Build();

        logger.LogInformation("Created version {Version} of notification {NotificationReferenceNumber}",
            notification.Version, notification.ReferenceNumber);

        return new GeneratorResult([clearanceRequest, notification, alvsDecision, updatedNotification]);
    }
}
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.ChedP;

public class DuplicateDecisionScenarioGenerator(ILogger<DuplicateDecisionScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithSimpleCommodity("1604142800", "Skipjack Tuna", 300)
            .WithInspectionStatus(InspectionRequiredEnum.NotRequired) //NB, the examples in the redacted data are title case, but code is uppercase CDMS-210
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        
        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithTunaItem()
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecision = GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithTunaChecks()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest, notification, alvsDecision, alvsDecision]);
    }
}
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

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
            .WithInspectionStatus("NOTREQUIRED") //NB, the examples in the redacted data are title case, but code is uppercase CDMS-210
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        // TODO - check with Matt what a sensible checkCode, decision & other fields we need to 
        // implement a 'real world' test here
        var checkCode = "H2019";
        var decisionCode = "H01";

        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithItem("N853", "16041421", "Tuna ROW CHEDP", 900, checkCode)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecision = GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithItemAndCheck(1, checkCode, decisionCode)
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest, notification, alvsDecision, alvsDecision]);
    }
}
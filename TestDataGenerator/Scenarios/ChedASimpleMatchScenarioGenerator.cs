using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;

namespace TestDataGenerator.Scenarios;

public class ChedASimpleMatchScenarioGenerator(ILogger<ChedASimpleMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = GetNotificationBuilder("cheda-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithDispatchCountryCode(notification.PartOne!.Route!.TransitingStates!.FirstOrDefault())
            .WithEntryVersionNumber()
            .ValidateAndBuild();

        var finalisation = GetFinalisationBuilder("finalisation")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(2), randomTime: false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithDecisionVersionNumber()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, notification, finalisation]);
    }
}
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios.ChedP;

public class ClearLinksDueToMrnDocumentRefChange(ILogger<ClearLinksDueToMrnDocumentRefChange> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = BuilderHelpers.GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, item)
            .WithSimpleCommodity("1604142800", "Skipjack Tuna", 300)
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}", 
            notification.ReferenceNumber);

        var clearanceRequestBuilder = BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithTunaItem();
            
        var clearanceRequest = clearanceRequestBuilder.ValidateAndBuild();
        
        var updatedClearanceRequest = clearanceRequestBuilder
            .Clone()
            .WithEntryVersionNumber(2)
            .WithCreationDate(entryDate.AddHours(4), false)
            .WithItemDocumentRef("GBCHD2024.001239999999") // Updating to trigger a no match
            .Build();
    
        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest, notification, updatedClearanceRequest]);
    }
}
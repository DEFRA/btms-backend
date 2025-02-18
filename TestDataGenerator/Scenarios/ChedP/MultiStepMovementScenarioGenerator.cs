using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios.ChedP;

public class MultiStepMovementScenarioGenerator(ILogger<MultiStepScenarioGenerator> logger) : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification1Builder = BuilderHelpers.GetNotificationBuilder("chedp-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, 1)
            .WithSimpleCommodity("1604142800", "Skipjack Tuna", 300)
            .WithInspectionStatus(InspectionRequiredEnum.NotRequired) //NB, the examples in the redacted data are title case, but code is uppercase CDMS-210
            .WithVersionNumber();

        var notification2 = notification1Builder
            .Clone()
            .WithReferenceNumber(ImportNotificationTypeEnum.Cvedp, scenario, entryDate, 2)
            .Build();

        var notification1 = notification1Builder.Build();

        logger.LogInformation("Created Notifications {Notification1ReferenceNumber}, {Notification2ReferenceNumber}",
            notification1.ReferenceNumber, notification2.ReferenceNumber);

        var clearanceRequestBuilder = BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate.AddHours(2), false)
            .WithArrivalDateTimeOffset(notification1.PartOne!.ArrivalDate, notification1.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification1.ReferenceNumber!)
            .WithEntryVersionNumber(1)
            .WithTunaItem();

        var clearanceRequest = clearanceRequestBuilder
            .ValidateAndBuild();

        var clearanceRequestv2 = clearanceRequestBuilder
            .Clone()
            .WithEntryVersionNumber(2)
            .WithCreationDate(entryDate.AddHours(4), false)
            .WithItemDocumentRef("GBCHD2024.001239999999") // Updating to trigger a no match
            .Build();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest, notification1, notification2, clearanceRequestv2]);
    }
}
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios;

public class ChedASimpleMatchScenarioGenerator(ILogger<ChedASimpleMatchScenarioGenerator> logger) : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = BuilderHelpers.GetNotificationBuilder("cheda-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {NotificationReferenceNumber}",
            notification.ReferenceNumber);

        var clearanceRequest = BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate)
            .WithArrivalDateTimeOffset(notification.PartOne!.ArrivalDate, notification.PartOne!.ArrivalTime)
            .WithReferenceNumberOneToOne(notification.ReferenceNumber!)
            .WithDispatchCountryCode(notification.PartOne!.Route!.TransitingStates!.FirstOrDefault())
            .WithEntryVersionNumber()
            .ValidateAndBuild();

        var finalisation = BuilderHelpers.GetFinalisationBuilder("finalisation")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(2), randomTime: false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithDecisionVersionNumber()
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var messages = new List<object>() { clearanceRequest, notification, finalisation };

        return new GeneratorResult(ModifyMessages(messages).ToArray());
    }
}

public class ChedASimpleMatchFixedDatesScenarioGenerator(ILogger<ChedASimpleMatchScenarioGenerator> logger)
    : ChedASimpleMatchScenarioGenerator(logger)
{
    protected override List<object> ModifyMessages(List<object> messages)
    {
        messages.ForEach(m =>
        {
            if (m is not ImportNotification notification) return;
            notification.PartOne!.DepartureDate = new DateOnly(2024, 12, 1);
            notification.PartOne!.DepartureTime = new TimeOnly(10, 10, 0);
        });

        return messages;
    }
}
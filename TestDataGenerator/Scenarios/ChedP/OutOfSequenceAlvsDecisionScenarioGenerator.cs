using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.ChedP;

public class OutOfSequenceAlvsDecisionScenarioGenerator(ILogger<OutOfSequenceAlvsDecisionScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = this
            .GetSimpleNotification(scenario, item, entryDate, config)
            .ValidateAndBuild();

        logger.LogInformation("Created Notification {NotificationReferenceNumber}",
            notification.ReferenceNumber);

        var clearanceRequest = this
            .GetSimpleClearanceRequest(scenario, item, entryDate, config, notification)
            .ValidateAndBuild();

        logger.LogInformation("Created Clearance Request {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecisionBuilder = this
            .GetSimpleDecision(scenario, item, entryDate, config, notification, clearanceRequest);

        var alvsDecisionV2 = alvsDecisionBuilder
            .Clone()
            .WithDecisionVersionNumber(2)
            .ValidateAndBuild();

        var alvsDecision = alvsDecisionBuilder
            .ValidateAndBuild();

        logger.LogInformation("Created Decision {EntryReference} {DecisionNumber}", alvsDecision.Header!.EntryReference, alvsDecision.Header!.DecisionNumber);
        logger.LogInformation("Created Decision {EntryReference} {DecisionNumber}", alvsDecisionV2.Header!.EntryReference, alvsDecision.Header!.DecisionNumber);

        return new GeneratorResult([clearanceRequest, notification, alvsDecisionV2, alvsDecision]);
    }
}
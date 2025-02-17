using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;

namespace TestDataGenerator.Scenarios.ChedP;

public class OutOfSequenceAlvsDecisionScenarioGenerator(ILogger<OutOfSequenceAlvsDecisionScenarioGenerator> logger) : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = this
            .GetSimpleNotification(scenario, item, entryDate, config)
            .ValidateAndBuild();

        var clearanceRequest = this
            .GetSimpleClearanceRequest(scenario, item, entryDate, config, notification)
            .ValidateAndBuild();

        logger.LogInformation("Created Notification {NotificationReference}, Clearance Request {ClearanceRequest}",
            notification.ReferenceNumber, clearanceRequest.Header!.EntryReference);

        var alvsDecisionBuilder = this
            .GetSimpleDecision(scenario, item, entryDate, config, notification, clearanceRequest);

        var alvsDecisionV2 = alvsDecisionBuilder
            .Clone()
            .WithDecisionVersionNumber(2)
            .ValidateAndBuild();

        var alvsDecision = alvsDecisionBuilder
            .ValidateAndBuild();

        logger.LogInformation("Created Decision v1 {EntryReference} {DecisionNumber}, v2 {EntryReference2} {DecisionNumber2}",
            alvsDecision.Header!.EntryReference, alvsDecision.Header!.DecisionNumber,
            alvsDecisionV2.Header!.EntryReference, alvsDecisionV2.Header!.DecisionNumber);

        return new GeneratorResult([clearanceRequest, notification, alvsDecisionV2, alvsDecision]);
    }
}
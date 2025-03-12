using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Extensions;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios;

public class SingleCrWithFinalisationValidationErrorsScenarioGenerator(ILogger<CrNoMatchNoChecksScenarioGenerator> logger) : ScenarioGenerator(logger)
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = BuilderHelpers.GetNotificationBuilder("cheda-one-commodity")
            .WithCreationDate(entryDate)
            .WithRandomArrivalDateTime(config.ArrivalDateRange)
            .WithReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item)
            .WithVersionNumber()
            .ValidateAndBuild();

        var clearanceRequest = NoMatchExtensions
            .SimpleClearanceRequest(scenario, item, entryDate, config)
            .WithTunaItem()
            .WithDispatchCountryCode("FR")
            .ValidateAndBuild();

        Logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var finalisation = BuilderHelpers.GetFinalisationBuilder("finalisation")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(2), randomTime: false)
            .WithReferenceNumber(notification.ReferenceNumber!)
            .WithDecisionVersionNumber()
            .ValidateAndBuild();

        finalisation.Header.FinalState = null!;

        return new GeneratorResult([clearanceRequest, finalisation]);
    }
}
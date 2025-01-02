using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios;

public class CrNoMatchNoDecisionScenarioGenerator(ILogger<CrNoMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var reference = DataHelpers.GenerateReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item);
        
        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate)
            .WithArrivalDateTimeOffset(DateTime.Today.ToDate(), DateTime.Now.ToTime())
            .WithReferenceNumberOneToOne(reference)
            .WithRandomItems(10, 100)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest]);
    }
}
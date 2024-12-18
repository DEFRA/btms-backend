using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios;

public class CrNoMatchNoChecksScenarioGenerator(ILogger<CrNoMatchNoChecksScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var clearanceRequest = GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate)
            .WithArrivalDateTimeOffset(DateTime.Today.ToDate(), DateTime.Now.ToTime())
            .WithReferenceNumber(DataHelpers.GenerateReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item))
            .WithItemNoChecks("N853", "16041421", "Tuna ROW CHEDP", 900)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest]);
    }
}
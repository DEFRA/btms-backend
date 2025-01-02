using Btms.Common.Extensions;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;

namespace TestDataGenerator.Scenarios;

public class CrNoMatchScenarioGenerator(ILogger<CrNoMatchScenarioGenerator> logger) : ScenarioGenerator
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

        // TODO - check with Matt what a sensible checkCode, decision & other fields we need to 
        // implement a 'real world' test here
        var checkCode = "H2019";
        var decisionCode = "H01";
        
        var alvsDecision = GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(reference)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithItemAndCheck(1, checkCode, decisionCode)
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, alvsDecision]);
    }
}
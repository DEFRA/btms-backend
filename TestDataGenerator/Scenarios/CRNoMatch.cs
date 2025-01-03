using Btms.Common.Extensions;
using Btms.Model;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging;
using TestDataGenerator.Helpers;
using Decision = Btms.Types.Alvs.Decision;

namespace TestDataGenerator.Scenarios;

public static class NoMatchExtensions
{
    
    public static AlvsClearanceRequest CompleteSimpleClearanceRequest(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        return NoMatchExtensions
            .SimpleClearanceRequest(scenario, item, entryDate, config)
            .WithTunaItem()
            .ValidateAndBuild();
    }
    
    public static ClearanceRequestBuilder<AlvsClearanceRequest> SimpleClearanceRequest(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        return BuilderHelpers.GetClearanceRequestBuilder("cr-one-item")
            .WithCreationDate(entryDate)
            .WithArrivalDateTimeOffset(DateTime.Today.ToDate(), DateTime.Now.ToTime())
            .WithReferenceNumberOneToOne(DataHelpers.GenerateReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario,
                entryDate, item))
            .WithEntryVersionNumber();
    }

    public static ClearanceRequestBuilder<AlvsClearanceRequest> WithTunaItem(this ClearanceRequestBuilder<AlvsClearanceRequest> builder)
    {
        return builder
            .WithItemNoChecks("N853", "16041421", "Tuna ROW CHEDP", 900);
    }

    public static MatchIdentifier DocumentReferenceFromFirstDoc(this AlvsClearanceRequest clearanceRequest)
    {
        return MatchIdentifier.FromCds(clearanceRequest
            .Items?
            .First()
            .Documents?
            .First()
            .DocumentReference!);
    }

    public static DecisionBuilder<Decision> GetDecisionBuilder(
        this AlvsClearanceRequest clearanceRequest)
    {
        var reference = clearanceRequest
            .DocumentReferenceFromFirstDoc();

        // TODO - check with Matt what a sensible checkCode, decision & other fields we need to 
        // implement a 'real world' test here
        var checkCode = "H2019";
        var decisionCode = "H01";

        return BuilderHelpers.GetDecisionBuilder("decision-one-item")
            .WithCreationDate(clearanceRequest.ServiceHeader!.ServiceCallTimestamp!.Value.AddHours(1), false)
            .WithReferenceNumber(reference)
            .WithEntryVersionNumber(1)
            .WithDecisionVersionNumber()
            .WithItemAndCheck(1, checkCode, decisionCode);

    }
}

public class CrNoMatchSingleItemWithDecisionScenarioGenerator(ILogger<CrNoMatchNoChecksScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var clearanceRequest = NoMatchExtensions
            .SimpleClearanceRequest(scenario, item, entryDate, config)
            .WithTunaItem()
            .WithDispatchCountryCode("FR")
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecision = clearanceRequest
            .GetDecisionBuilder()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, alvsDecision]);
    }
}
public class CrNoMatchNoChecksScenarioGenerator(ILogger<CrNoMatchNoChecksScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var clearanceRequest = NoMatchExtensions
            .CompleteSimpleClearanceRequest(scenario, item, entryDate, config);

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest]);
    }
}

public class CrNoMatchNoDecisionScenarioGenerator(ILogger<CrNoMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var reference = DataHelpers.GenerateReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item);
        
        var clearanceRequest = NoMatchExtensions
            .SimpleClearanceRequest(scenario, item, entryDate, config)
            .WithReferenceNumberOneToOne(reference)
            .WithRandomItems(10, 100)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        return new GeneratorResult([clearanceRequest]);
    }
}

public class CrNoMatchScenarioGenerator(ILogger<CrNoMatchScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        // var reference = DataHelpers.GenerateReferenceNumber(ImportNotificationTypeEnum.Cveda, scenario, entryDate, item);

        var clearanceRequest = NoMatchExtensions
            .SimpleClearanceRequest(scenario, item, entryDate, config)
            .WithRandomItems(10, 100)
            .ValidateAndBuild();

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecision = clearanceRequest
            .GetDecisionBuilder()
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, alvsDecision]);
    }
}


public class CrNonContiguousDecisionsScenarioGenerator(ILogger<CrNonContiguousDecisionsScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var clearanceRequest = NoMatchExtensions
            .CompleteSimpleClearanceRequest(scenario, item, entryDate, config);

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecisionBuilder = clearanceRequest
            .GetDecisionBuilder()
            .WithDecisionVersionNumber(1);

        var alvsDecision = alvsDecisionBuilder
            .ValidateAndBuild();
        
        var alvsDecision2 = alvsDecisionBuilder
            .Clone()
            .WithDecisionVersionNumber(3)
            .ValidateAndBuild();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, alvsDecision, alvsDecision2]);
    }
}


public class CrDecisionWithoutV1ScenarioGenerator(ILogger<CrDecisionWithoutV1ScenarioGenerator> logger) : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var clearanceRequest = NoMatchExtensions
            .CompleteSimpleClearanceRequest(scenario, item, entryDate, config);

        logger.LogInformation("Created {EntryReference}", clearanceRequest.Header!.EntryReference);

        var alvsDecision = clearanceRequest
            .GetDecisionBuilder()
            .WithDecisionVersionNumber(2)
            .Build();
        
        logger.LogInformation("Created {EntryReference}", alvsDecision.Header!.EntryReference);
        
        return new GeneratorResult([clearanceRequest, alvsDecision]);
    }
}
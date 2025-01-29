using System.Collections;
using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class FinalisationTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    
    static readonly List<object[]> scenarios = [[typeof(Mrn24Gbdej9V2Od0Bhar0ScenarioGenerator), FinalState.CancelledAfterArrival, false],
        [typeof(Mrn24Gbde8Olvkzxsyar1ScenarioGenerator), FinalState.Cleared, false],
        [typeof(Mrn24Gbdej9V2Od0Bhar0DestroyedScenarioGenerator), FinalState.Destroyed, false],
        [typeof(Mrn24Gbdej9V2Od0Bhar0ManualActionScenarioGenerator), FinalState.CancelledAfterArrival, true]];

    public static IEnumerable<object[]> Scenarios()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0]];
        }
    }
    
    public static IEnumerable<object[]> ScenariosWithFinalState()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0], scenario[1]];
        }
    }
    
    public static IEnumerable<object[]> ScenariosWithManualAction()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0], scenario[2]];
        }
    }
    
    [Theory]
    [MemberData(nameof(Scenarios))]
    public void FinalisedShouldBeSet(Type generator)
    {
        EnsureEnvironmentInitialised(generator);
        
         Client
             .GetSingleMovement()
             .Finalised
             .Should().NotBeNull();
    }
    
    [Theory]
    [MemberData(nameof(ScenariosWithFinalState))]
    public void FinalisationFinalStateShouldBeCorrect(Type generator, FinalState finalState)
    {
        EnsureEnvironmentInitialised(generator);
        
        Client
            .GetSingleMovement()
            .Finalisation!.FinalState
            .Should().Be(finalState);
    }
    
    [Theory]
    [MemberData(nameof(ScenariosWithManualAction))]
    public void FinalisationManualActionShouldBeCorrect(Type generator, bool manualAction)
    {
        EnsureEnvironmentInitialised(generator);

        Client
            .GetSingleMovement()
            .Finalisation!.ManualAction
            .Should().Be(manualAction);
    }
}
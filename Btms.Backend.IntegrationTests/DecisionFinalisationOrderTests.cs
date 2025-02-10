using Btms.Model.Cds;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

/// <summary>
/// At the moment the integration tests mostly process everything sequentially in the expected order
/// however in reality, sometimes we process decisions and finalisations before the import notifications,
/// so we want to ensure we cover these with tests
/// </summary>
/// <param name="output"></param>
[Trait("Category", "Integration")]
public class DecisionFinalisationOrderTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    
    static readonly List<object[]> scenarios = [[typeof(Mrn24Gbde8Olvkzxsyar1ScenarioGenerator), "C03"],
        [typeof(Mrn24Gbde8Olvkzxsyar1ImportNotificationsAtEndScenarioGenerator), "C03"]
    ];

    public static IEnumerable<object[]> Scenarios()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0]];
        }
    }
    
    public static IEnumerable<object[]> ScenariosWithExpectedDecisionCode()
    {
        foreach (var scenario in scenarios)
        {
            yield return [scenario[0], scenario[1]];
        }
    }
    
    [Theory]
    [MemberData(nameof(ScenariosWithExpectedDecisionCode))]
    public void ShouldHaveCorrectDecisionCode(Type generatorType, string expectedDecisionCode)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, expectedDecisionCode);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionCode(expectedDecisionCode);
    }
    
    private void CheckDecisionCode(string expectedDecisionCode)
    {
        var movement =
            Client
                .GetSingleMovement();
        
        TestOutputHelper.WriteLine("MRN {0}, expectedDecisionCode {1}", movement.EntryReference, expectedDecisionCode);
        
        movement
            .Decisions!.MaxBy(d => d.ServiceHeader!.ServiceCalled)?
            .Items!.First()
            .Checks!.First()
            .DecisionCode.Should().Be(expectedDecisionCode);
    }
    
    [Theory]
    [MemberData(nameof(Scenarios))]
    public void ShouldHaveMaximumBtmsDecisionCode(Type generatorType)
    {
        EnsureEnvironmentInitialised(generatorType);
        
        var movement = Client
            .GetSingleMovement();
        
        movement
            .AlvsDecisionStatus.Context.DecisionComparison!.BtmsDecisionNumber
            .Should()
            .Be(movement.Decisions.Max(d => d.Header.DecisionNumber));
    }
    
    [Theory]
    [MemberData(nameof(Scenarios))]
    public void ShouldHaveMaximumAlvsDecisionCode(Type generatorType)
    {
        EnsureEnvironmentInitialised(generatorType);
        
        var movement = Client
            .GetSingleMovement();
        
        movement.AlvsDecisionStatus.Context.AlvsDecisionNumber
            .Should()
            .Be(movement.AlvsDecisionStatus.Decisions.Max(d => d.Decision.Header.DecisionNumber));
    }
    
    [Theory]
    [MemberData(nameof(Scenarios))]
    public void ShouldHaveDecisionMatched(Type generatorType)
    {
        EnsureEnvironmentInitialised(generatorType);
        
        var movement = Client
            .GetSingleMovement();
        
        movement.AlvsDecisionStatus.Context.DecisionComparison?.DecisionMatched
            .Should()
            .BeTrue();
    }
    
    [Theory]
    [MemberData(nameof(Scenarios))]
    public void ShouldHaveDecisionStatus(Type generatorType)
    {
        EnsureEnvironmentInitialised(generatorType);
        
        var movement = Client
            .GetSingleMovement();
        
        movement.AlvsDecisionStatus.Context.DecisionComparison?.DecisionStatus
            .Should()
            .Be(DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs);
    }

}
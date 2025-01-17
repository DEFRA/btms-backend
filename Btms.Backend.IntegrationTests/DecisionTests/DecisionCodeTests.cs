using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class DecisionCodeTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(ChedAh01ScenarioScenarioGenerator), "H01")]
    [InlineData(typeof(ChedPh01ScenarioScenarioGenerator), "H01")]
    [InlineData(typeof(ChedDh01ScenarioScenarioGenerator), "H01")]
    [InlineData(typeof(ChedAh02ScenarioScenarioGenerator), "H02")]
    [InlineData(typeof(ChedPh02ScenarioScenarioGenerator), "H02")]
    [InlineData(typeof(ChedDh02ScenarioScenarioGenerator), "H02")]
    [InlineData(typeof(ChedAc03ScenarioScenarioGenerator), "C03")]
    [InlineData(typeof(ChedPc03ScenarioScenarioGenerator), "C03")]
    [InlineData(typeof(ChedDc03ScenarioScenarioGenerator), "C03")]
    public void ShouldHaveCorrectDecisionCode(Type generatorType, string decisionCode)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, decisionCode);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionCode(decisionCode);
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
}
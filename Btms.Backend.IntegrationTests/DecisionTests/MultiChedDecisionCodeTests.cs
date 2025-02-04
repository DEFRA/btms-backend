using Btms.Model;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class MultiChedDecisionTest(ITestOutputHelper output)
    : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(MultiChedPMatchScenarioGenerator), "H02")]
    [InlineData(typeof(MultiChedAMatchScenarioGenerator), "C03")]
    [InlineData(typeof(MultiChedDMatchScenarioGenerator), "C03")]
    public void MultiChed_DecisionCode(Type generatorType, string expectedDecision)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, expectedDecision);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionCode(expectedDecision);
    }
    
    [Theory (Skip = "Need to update data")]
    [InlineData(typeof(MultiChedPWorstCaseMatchScenarioGenerator), "N07")]
    [InlineData(typeof(MultiChedAWorstCaseMatchScenarioGenerator), "H02")]
    [InlineData(typeof(MultiChedDWorstCaseMatchScenarioGenerator), "N02")]
    public void MultiChed_WorstCaseDecisionCode(Type generatorType, string expectedDecision)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, expectedDecision);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionCode(expectedDecision);
    }

    [Theory]
    [InlineData(typeof(Mrn24Gbdev7Bgq1L0Oar4ScenarioGenerator))]
    public void MultiChed_DecisionCode_ShouldContainAllItems(Type generatorType)
    {
        EnsureEnvironmentInitialised(generatorType);

        var movement = Client.AsJsonApiClient().Get("api/movements").GetResourceObjects<Movement>().Single();

        foreach (var movementDecision in movement.Decisions)
        {
            movementDecision.Items.Length.Should().Be(movement.Items.Count);
        }
    }

    private void CheckDecisionCode(string expectedDecision)
    {
        var decision = "";
        Client.AsJsonApiClient().Get("api/movements").GetResourceObjects<Movement>().Single().Decisions
            .OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
            .All(i =>
            {
                decision = i.Checks!.First().DecisionCode!;

                return decision.Equals(expectedDecision);
            }).Should().BeTrue($"Expected {expectedDecision}. Actually {{0}}", decision);
    }
}
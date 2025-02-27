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
    // Data has been updated to exercise CDMS-345. Data has a in-progress notification after the Validated notification.
    [Theory]
    [InlineData(typeof(MultiChedPMatchScenarioGenerator), "C03", "C03", "C03")]
    [InlineData(typeof(MultiChedAMatchScenarioGenerator), "C03", "C03")]
    [InlineData(typeof(MultiChedDMatchScenarioGenerator), "C03", "C03", "C03")]
    public void MultiChed_DecisionCode(Type generatorType, params string[] expectedDecision)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, expectedDecision);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionCode(expectedDecision);
    }

    [Theory(Skip = "Need to update data")]
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

    private void CheckDecisionCode(params string[] expectedDecision)
    {
        Client.AsJsonApiClient().Get("api/movements").GetResourceObjects<Movement>().Single()
            .Decisions!.MaxBy(d => d.ServiceHeader!.ServiceCalled)?
            .Items!.SelectMany(i => i.Checks!)
            .Select(c => c.DecisionCode)
            .Should().Equal(expectedDecision);
    }
}
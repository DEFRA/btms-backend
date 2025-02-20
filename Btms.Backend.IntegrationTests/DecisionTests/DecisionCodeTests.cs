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
    [InlineData(typeof(ChedAh01ScenarioGenerator1), "H01", "H01", "H01", "H01", "H01")]
    [InlineData(typeof(ChedAh01ScenarioGenerator2), "H01", "H01", "H01", "H01", "H01")]
    [InlineData(typeof(ChedAh01ScenarioGenerator3), "H01", "H01", "H01", "H01", "H01")]
    [InlineData(typeof(ChedAh01ScenarioGenerator4), "H01", "H01", "H01", "H01", "H01")]
    [InlineData(typeof(ChedDh01ScenarioGenerator1), "H01")]
    [InlineData(typeof(ChedDh01ScenarioGenerator2), "H01")]
    [InlineData(typeof(ChedDh01ScenarioGenerator3), "H01")]
    [InlineData(typeof(ChedDh01ScenarioGenerator4), "H01")]
    [InlineData(typeof(ChedPh01ScenarioGenerator1), "H01", "H01")]
    [InlineData(typeof(ChedPh01ScenarioGenerator2), "H01", "H01")]
    [InlineData(typeof(ChedPh01ScenarioGenerator3), "H01", "H01")]
    [InlineData(typeof(ChedPh01ScenarioGenerator4), "H01", "H01")]
    [InlineData(typeof(ChedAh02ScenarioGenerator1), "H02", "H02", "H02", "H02", "H02")]
    [InlineData(typeof(ChedAh02ScenarioGenerator2), "H02", "H02", "H02", "H02", "H02")]
    [InlineData(typeof(ChedDh02ScenarioGenerator1), "H02")]
    [InlineData(typeof(ChedDh02ScenarioGenerator2), "H02")]
    [InlineData(typeof(ChedPh02ScenarioGenerator1), "H02", "H02")]
    [InlineData(typeof(ChedPh02ScenarioGenerator2), "H02", "H02")]
    [InlineData(typeof(ChedAc03ScenarioGenerator), "C03", "C03", "C03", "C03", "C03")]
    [InlineData(typeof(ChedPc03ScenarioGenerator), "C03", "C03")]
    [InlineData(typeof(ChedDc03ScenarioGenerator), "C03")]
    [InlineData(typeof(ChedAc05ScenarioGenerator), "C05", "C05", "C05", "C05", "C05")]
    [InlineData(typeof(ChedAc06ScenarioGenerator), "C06", "C06", "C06", "C06", "C06")]
    [InlineData(typeof(ChedPc06ScenarioGenerator), "C06", "C06")]
    [InlineData(typeof(ChedAn02ScenarioGenerator), "N02", "N02", "N02", "N02", "N02")]
    [InlineData(typeof(ChedAn04ScenarioGenerator), "N04", "N04", "N04", "N04", "N04")]
    [InlineData(typeof(ChedDn02ScenarioGenerator), "N02")]
    [InlineData(typeof(ChedDn04ScenarioGenerator), "N04")]
    [InlineData(typeof(ChedPn02ScenarioGenerator), "N02", "N02")]
    [InlineData(typeof(ChedPn03ScenarioGenerator), "N03", "N03")]
    [InlineData(typeof(ChedPn04ScenarioGenerator), "N04", "N04")]
    [InlineData(typeof(ChedPn07ScenarioGenerator), "N07", "N07")]
    [InlineData(typeof(MissingChedScenarioGenerator), "X00", "X00")]
    [InlineData(typeof(IuuScenarioGenerator), "E89", "E89")] //"C03", "C07")]
    [InlineData(typeof(Mrn24Gbdzsrxdxtbvkar6ScenarioGenerator), "C03", "C03", "C03")]
    [InlineData(typeof(Mrn24Gbei6Oisht38Mar9ScenarioGenerator), "H02", "H02")]
    public void ShouldHaveCorrectDecisionCode(Type generatorType, params string[] expectedDecisionCode)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, expectedDecisionCode);
        EnsureEnvironmentInitialised(generatorType);
        CheckDecisionCode(expectedDecisionCode);
    }

    private void CheckDecisionCode(params string[] expectedDecisionCode)
    {
        var movement =
            Client
                .GetSingleMovement();

        TestOutputHelper.WriteLine("MRN {0}, expectedDecisionCode {1}", movement.EntryReference, expectedDecisionCode);

        movement
            .Decisions!.MaxBy(d => d.ServiceHeader!.ServiceCalled)?
            .Items!.SelectMany(i => i.Checks!)
            .Select(c => c.DecisionCode)
            .Should().Equal(expectedDecisionCode);
    }

    [Theory]
    [InlineData(typeof(Mrn24Gbeds4W7Dfrlmar0ScenarioGenerator), "C03")]
    public void ShouldProcess(Type generatorType, string expectedDecisionCode)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, expectedDecisionCode);
        EnsureEnvironmentInitialised(generatorType);

        var movement =
            Client
                .GetSingleMovement();

        movement
            .Decisions!.MaxBy(d => d.ServiceHeader!.ServiceCalled)?
            .Items!.Should().AllSatisfy(i =>
                i.Checks!.Should().AllSatisfy(c =>
                    c.DecisionCode.Should().Be(expectedDecisionCode)
                )
            );


    }
}
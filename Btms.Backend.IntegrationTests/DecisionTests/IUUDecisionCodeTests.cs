using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class IUUDecisionCodeTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(IuuNotCompletedScenarioGenerator), "X00")]
    [InlineData(typeof(IuuOkScenarioGenerator), "X00")] //"C07")]
    [InlineData(typeof(IuuNotCompliantScenarioGenerator), "X00")]
    [InlineData(typeof(IuunaScenarioGenerator), "X00")] //"C08")]
    [InlineData(typeof(NoIuuInfoScenarioGenerator), "X00")]
    public void ShouldHaveCorrectIuuDecisionCode(Type generatorType, string decisionCode)
    {
        TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, decisionCode);
        EnsureEnvironmentInitialised(generatorType);

        var movement = Client
            .GetSingleMovement();

        var lastDecision = movement.Decisions.OrderByDescending(x => x.ServiceHeader?.ServiceCalled).First();


        foreach (var item in lastDecision.Items!)
        {
            foreach (var itemCheck in item.Checks!)
            {
                if (itemCheck.CheckCode == "H224")
                {
                    itemCheck.DecisionCode.Should().Be(decisionCode);
                }
            }
        }
    }
}
using Btms.Model;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests;

[Trait("Category", "Integration")]
public class BasicDecisionCodeTests
{
    public class SingleChedH01Tests(ITestOutputHelper output)
        : ScenarioGeneratorBaseTest<AllChedsH01DecisionGenerator>(output)
    {
        [Fact]
        public void SingleChed_ShouldHaveH01DecisionCode()
        {
            CheckDecisionCode("H01", Client);
        }
    }
    
    public class SingleChedH02Tests(ITestOutputHelper output)
        : ScenarioGeneratorBaseTest<AllChedsH02DecisionGenerator>(output)
    {
        [Fact]
        public void SingleChed_ShouldHaveH02DecisionCode()
        {
            CheckDecisionCode("H02", Client);
        }
    }
    
    [Trait("Category", "Integration")]
    public class SingleChedDecisionTests(ITestOutputHelper output)
        : ScenarioGeneratorBaseTest<AllChedsC03DecisionGenerator>(output)
    {
        [Fact]
        public void SingleChed_ShouldHaveC03DecisionCode()
        {
            CheckDecisionCode("C03", Client);
        }
    }
    
    [Trait("Category", "Integration")]
    public class MultiChedDecisionTest(ITestOutputHelper output)
        : ScenarioGeneratorBaseTest<MultiChedPMatchScenarioGenerator>(output)
    {
        [Fact]
        public void MultiChed_ShouldHaveH02DecisionCode()
        {
            string decisionCode = "";
            var expectedDecision = "H02";
            Client.AsJsonApiClient().Get("api/movements").GetResourceObjects<Movement>().Single().Decisions
                .OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items!
                .All(i =>
                {
                    decisionCode = i.Checks!.First().DecisionCode!;

                    return decisionCode.Equals(expectedDecision);
                }).Should().BeTrue($"Expected {expectedDecision}. Actually {{0}}", decisionCode);
        }
    }
    
    private static void CheckDecisionCode(string expectedDecisionCode, BtmsClient client)
    {
        string decisionCode;
        string mrn;

        var movements = client.AsJsonApiClient().Get("api/movements").GetResourceObjects<Movement>();
        movements.Should().AllSatisfy(m =>
        {
            mrn = m.EntryReference;
            m.Decisions.OrderBy(x => x.ServiceHeader?.ServiceCalled).Last().Items.Should().AllSatisfy(i =>
            {
                i.Checks.Should().AllSatisfy(c =>
                {
                    decisionCode = c.DecisionCode!;
                    decisionCode.Should().Be(expectedDecisionCode,
                        $"Expected {expectedDecisionCode}. Actually {{0}}. MRN: {{1}}", decisionCode, mrn);
                });
            });
        });
    }
}
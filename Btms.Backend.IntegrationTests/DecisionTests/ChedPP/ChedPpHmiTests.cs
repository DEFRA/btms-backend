using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests.DecisionTests.ChedPP;


[Trait("Category", "Integration")]
public class ChedPpHmiTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(ChedPpHmiDecisionTestsScenarioGenerator), "24GBD447SAPD7NTAR9", "C03")]

    [InlineData(typeof(ChedPpHmiDecisionTestsScenarioGenerator), "24GBD69TMXZ2TYCAR9", "N02")]
    [InlineData(typeof(ChedPpHmiDecisionTestsScenarioGenerator), "24GBD69TMXZ2TYCAR8", "H01")]
    [InlineData(typeof(ChedPpHmiDecisionTestsScenarioGenerator), "24GBD69TMXZ2TYCAR7", "H02")]
     public void DecisionShouldHaveCorrectDecisionCodeForSingleNotification(Type generatorType, string mrn, string decisionCode)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, decisionCode);
        EnsureEnvironmentInitialised(generatorType);

        var movement = Client
            .GetMovementByMrn(mrn);

        var lastDecision = movement.Decisions.OrderByDescending(x => x.ServiceHeader?.ServiceCalled).First();


        foreach (var item in lastDecision.Items!)
        {
            foreach (var itemCheck in item.Checks!)
            {
                itemCheck.DecisionCode.Should().Be(decisionCode);
            }
        }
    }
    
  
}
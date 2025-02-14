using Btms.Model;
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

    //Movement With Different Ched Types, that caused a finder exceptions
    [InlineData(typeof(Mrn24Gbdy6Xff66H0Xar1ScenarioGenerator), "24GBDY6XFF66H0XAR1", "C03")]
    public void DecisionShouldHaveCorrectDecisionCodeForSingleNotification(Type generatorType, string mrn, string decisionCode)
    {
        base.TestOutputHelper.WriteLine("Generator : {0}, Decision Code : {1}", generatorType!.FullName, decisionCode);
        EnsureEnvironmentInitialised(generatorType);

        var apiResponse = Client
            .GetMovementByMrn(mrn);

        var movement = apiResponse.GetResourceObject<Movement>();

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
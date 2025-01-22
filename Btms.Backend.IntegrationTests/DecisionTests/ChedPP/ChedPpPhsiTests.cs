using Btms.Model.Auditing;
using Btms.Model.Cds;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;
using ImportNotificationTypeEnum = Btms.Model.Ipaffs.ImportNotificationTypeEnum;

namespace Btms.Backend.IntegrationTests.DecisionTests.ChedPP;


[Trait("Category", "Integration")]
public class ChedPpPhsiTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(ChedPpPhsiDecisionTestsScenarioGenerator), "24GBD70G9C5RSI5AR5", "C03")]
    [InlineData(typeof(ChedPpPhsiDecisionTestsScenarioGenerator), "24GBDC4TW6DUQYIAR5", "N02")]
    [InlineData(typeof(ChedPpPhsiDecisionTestsScenarioGenerator), "24GBDN3EZ714MBIAR9", "H01")]
    [InlineData(typeof(ChedPpPhsiDecisionTestsScenarioGenerator), "24GBDCVPBWY0GE9AR8", "H02")]
    ////[InlineData(typeof(ChedPpPhsiDecisionTestsScenarioGenerator), "24GBDB6P6GWPVLKAR7", "C03")] put scenario in once HMI has been implemented 
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
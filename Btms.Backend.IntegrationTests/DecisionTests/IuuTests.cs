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

namespace Btms.Backend.IntegrationTests.DecisionTests;


[Trait("Category", "Integration")]
public class IuuTests(ITestOutputHelper output) : MultipleScenarioGeneratorBaseTest(output)
{
    [Theory]
    [InlineData(typeof(ChedPIuuDecisionTestsScenarioGenerator), "24GBDE3CLN8AH8OAR0", "C07")] 
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
                if (itemCheck.CheckCode == "H224")
                {
                    itemCheck.DecisionCode.Should().Be(decisionCode);
                }
            }
        }
    }
    
  
}
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class AsbSmokeTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : BaseApiTests(factory, testOutputHelper, "AsbSmokeTests"), IClassFixture<ApplicationFactory>
{
    [Fact(Skip = "Do not run correctly on github at the moment")]
    public async Task AsbSmokeTest()
    {
        await base.ClearDb();
        var testGeneratorFixture = new TestGeneratorFixture(Factory.TestOutputHelper);
        var generatorResult = testGeneratorFixture.GenerateTestData<SimpleMatchScenarioGenerator>();

        foreach (var generatedResult in generatorResult)
        {
            switch (generatedResult.Message)
            {
                case AlvsClearanceRequest cr:
                    await ServiceBusHelper.PublishClearanceRequest(cr);
                    break;
                case Decision d:
                    await ServiceBusHelper.PublishDecision(d);
                    break;
                case ImportNotification n:
                    await ServiceBusHelper.PublishNotification(n);
                    break;
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(30));


        // Check Api
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientResponse.Data.Count.Should().Be(1);

        jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        jsonClientResponse.Data.Count.Should().Be(1);
    }
}
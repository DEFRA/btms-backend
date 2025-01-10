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
    [Fact]
    public async Task AsbSmokeTest()
    {
        await base.ClearDb();
        var testGeneratorFixture = new TestGeneratorFixture(Factory.TestOutputHelper);
        var generatorResult = testGeneratorFixture.GenerateTestData<SimpleMatchScenarioGenerator>();

        foreach (var generatedResult in generatorResult)
        {
            if (generatedResult.Message is AlvsClearanceRequest cr)
            {
                await ServiceBusHelper.PublishClearanceRequest(cr);
            }
            else if (generatedResult.Message is Decision d)
            {
                await ServiceBusHelper.PublishDecision(d);
            }
            else if (generatedResult.Message is ImportNotification n)
            {
                await ServiceBusHelper.PublishNotification(n);
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
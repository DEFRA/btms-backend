using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend.Fixtures;
using TestGenerator.IntegrationTesting.Backend.JsonApiClient;
using Xunit;
using Xunit.Abstractions;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class AsbSmokeTests : BaseApiTests, IClassFixture<ApplicationFactory>
{
    public AsbSmokeTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper, "AsbSmokeTests")
    {
        factory.ConfigureHostConfiguration = configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConsumerOptions:EnableAsbConsumers", "true" }
            });
        };
    }

    [Fact]
    public async Task AsbSmokeTest()
    {
        await ClearDb();
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

        ManyItemsJsonApiDocument jsonClientResponse;

        await ShouldEventually.Be(() =>
            {
                jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
                jsonClientResponse.Data.Count.Should().Be(1);

                return Task.CompletedTask;
            },
            retries: 30,
            wait: TimeSpan.FromSeconds(1));

        jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        jsonClientResponse.Data.Count.Should().Be(1);
    }
}
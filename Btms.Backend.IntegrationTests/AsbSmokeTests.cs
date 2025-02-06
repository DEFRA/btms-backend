using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TestDataGenerator.Helpers;
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
    public AsbSmokeTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory,
        testOutputHelper, "AsbSmokeTests")
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
    public async Task AsbSmokeTest_NotificationsAndMovements()
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

        ManyItemsJsonApiDocument document;

        ShouldEventually.Be(() =>
            {
                document = Client.AsJsonApiClient().Get("api/import-notifications");
                document.Data.Count.Should().Be(1);
            },
            retries: 30,
            wait: TimeSpan.FromSeconds(1));

        document = Client.AsJsonApiClient().Get("api/movements");
        document.Data.Count.Should().Be(1);
    }

    [Fact]
    public async Task AsbSmokeTest_Gmrs()
    {
        await ClearDb();
        var gmr = BuilderHelpers.GetGmrBuilder("asb-gmr")
            .With(x => x.State, StateEnum.Finalised)
            .With(x => x.UpdatedSource, DateTime.Now)
            .ValidateAndBuild();
        
        await ServiceBusHelper.PublishGmr(gmr);
        
        ManyItemsJsonApiDocument document;

        ShouldEventually.Be(() =>
            {
                document = Client.AsJsonApiClient().Get("api/gmrs");
                document.Data.Count.Should().Be(1);
            },
            retries: 30,
            wait: TimeSpan.FromSeconds(1));
        
        gmr = BuilderHelpers.GetGmrBuilder("asb-gmr")
            .With(x => x.State, StateEnum.Embarked)
            .With(x => x.UpdatedSource, DateTime.Now)
            .ValidateAndBuild();
        
        await ServiceBusHelper.PublishGmr(gmr);
        
        ShouldEventually.Be(() =>
            {
                document = Client.AsJsonApiClient().Get("api/gmrs");
                document.Data.Count.Should().Be(1);
                document.Data.First().Attributes?["state"]?.ToString().Should().Be("Embarked");
            },
            retries: 30,
            wait: TimeSpan.FromSeconds(1));
    }
}
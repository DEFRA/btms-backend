using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Gvms;
using FluentAssertions;
using TestDataGenerator.Helpers;
using TestDataGenerator.Scenarios.ChedP;
using TestGenerator.IntegrationTesting.Backend.JsonApiClient;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class AsbSmokeTests : BaseApiTests, IClassFixture<ApplicationFactory>
{
    public AsbSmokeTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory,
        testOutputHelper, "AsbSmokeTests")
    {
        factory.EnableAzureServiceBusConsumers = true;
    }

    [Fact]
    public async Task AsbSmokeTest_NotificationsAndMovements()
    {
        await ClearDb();
        await PublishMessagesToAzureServiceBus<SimpleMatchScenarioGenerator>();

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

        await AzureServiceBusHelper.PublishGmr(gmr);

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

        await AzureServiceBusHelper.PublishGmr(gmr);

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
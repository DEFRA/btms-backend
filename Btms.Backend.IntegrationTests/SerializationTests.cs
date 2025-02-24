using System.Text.Json;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Types.Alvs;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend.JsonApiClient;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class SerializationTests : BaseApiTests, IClassFixture<ApplicationFactory>
{
    public SerializationTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
        factory.InternalQueuePublishWillBlock = true;
    }

    [Fact]
    public async Task ImportNotifications_Serialization_AsExpected()
    {
        await ClearDb();
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is AlvsClearanceRequest);
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is ImportNotification);

        var document = Client.AsJsonApiClient().Get("api/import-notifications");

        AssertDocument(document);
    }

    [Fact]
    public async Task Movements_Serialization_AsExpected()
    {
        await ClearDb();
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is ImportNotification);
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is AlvsClearanceRequest);

        var document = Client.AsJsonApiClient().Get("api/movements");

        AssertDocument(document);
    }

    private static void AssertDocument(ManyItemsJsonApiDocument document)
    {
        document.Data.Count.Should().BeGreaterThan(0);
        document.Data.First().Attributes.Should().ContainKey("updated");
        document.Data.First().Attributes.Should().ContainKey("updatedEntity");

        var updatedEntity = ParseDateTime(document, "updatedEntity");
        var updated = ParseDateTime(document, "updated");

        updatedEntity.Should().BeAfter(updated);
    }

    private static DateTime ParseDateTime(ManyItemsJsonApiDocument document, string field)
    {
        return DateTime.Parse(((JsonElement)(document.Data.First().Attributes?[field] ??
                                              throw new InvalidOperationException("Cannot be null"))).ToString());
    }
}
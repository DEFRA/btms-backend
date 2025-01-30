using System.Text.Json;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using FluentAssertions;
using TestGenerator.IntegrationTesting.Backend.JsonApiClient;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class SerializationTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper)
    : BaseApiTests(factory, testOutputHelper), IClassFixture<ApplicationFactory>
{
    [Fact]
    public async Task ImportNotifications_Serialization_AsExpected()
    {
        await ClearDb();
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });

        var document = Client.AsJsonApiClient().Get("api/import-notifications");

        AssertDocument(document);
    }

    [Fact]
    public async Task Movements_Serialization_AsExpected()
    {
        await ClearDb();
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
        });

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
        return DateTime.Parse(((JsonElement) (document.Data.First().Attributes?[field] ??
                                              throw new InvalidOperationException("Cannot be null"))).ToString());
    }
}
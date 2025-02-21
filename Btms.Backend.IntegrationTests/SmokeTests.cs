using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using Btms.Model;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using FluentAssertions;
using TestDataGenerator.Scenarios.SpecificFiles;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class SmokeTests : BaseApiTests, IClassFixture<ApplicationFactory>
{
    private readonly JsonSerializerOptions _jsonOptions;

    public SmokeTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        _jsonOptions.PropertyNameCaseInsensitive = true;
        
        factory.InternalQueuePublishWillBlock = true;
    }

    [Fact]
    public async Task CancelSyncJob()
    {
        await ClearDb();
        var (response, jobId) = await Client.StartJob(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "RootFolder"
        }, "/sync/import-notifications");

        var cancelJobResponse = await Client.CancelJob(jobId);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        cancelJobResponse.IsSuccessStatusCode.Should().BeTrue(cancelJobResponse.StatusCode.ToString());

        var jobResponse = await Client.GetJob(jobId);
        var syncJob = await jobResponse.Content.ReadFromJsonAsync<SyncJobResponse>(_jsonOptions);
        syncJob?.Status.Should().Be(SyncJobStatus.Cancelled);
    }

    [Fact]
    public async Task SyncNotifications()
    {
        await ClearDb();
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is ImportNotification);

        Factory.GetDbContext().Notifications.Count().Should().Be(5);

        var jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientResponse.Data.Count.Should().Be(5);

        Client.GetFirstImportNotification()
            .AuditEntries
            .First(a => a.Status == "Created").Id
            .Should().Be("CHEDA.GB.2024.1041389");
    }

    [Fact]
    public async Task SyncDecisions()
    {
        await ClearDb();
        await SyncClearanceRequests();
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is Decision);

        var existingMovement = await Factory.GetDbContext().Movements.Find("CHEDPGB20241039875A5");
        existingMovement.Should().NotBeNull();
        existingMovement?.Items[0].Checks.Should().NotBeNull();
        existingMovement?.Items[0].Checks?.Length.Should().Be(1);
        existingMovement?.Items[0].Checks?[0].CheckCode.Should().Be("H234");
        existingMovement?.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");

        var document = Client.AsJsonApiClient().GetById("CHEDPGB20241039875A5", "api/movements");
        var movement = document.GetResourceObject<Movement>();
        movement.Items[0].Checks?.Length.Should().Be(1);
        movement.Items[0].Checks?[0].CheckCode.Should().Be("H234");
        movement.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");
    }

    [Fact]
    public async Task SyncClearanceRequests()
    {
        await ClearDb();
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is AlvsClearanceRequest);

        Factory.GetDbContext().Movements.Count().Should().Be(5);

        var document = Client.AsJsonApiClient().Get("api/movements");
        document.Data.Count.Should().Be(5);
    }

    [Fact]
    public async Task SyncGmrs()
    {
        await ClearDb();
        await PublishMessagesToInMemoryTopics<SmokeTestScenarioGenerator>(x => x is SearchGmrsForDeclarationIdsResponse);

        Factory.GetDbContext().Gmrs.Count().Should().Be(3);

        var document = Client.AsJsonApiClient().Get("api/gmrs");
        document.Data.Count.Should().Be(3);
    }
}
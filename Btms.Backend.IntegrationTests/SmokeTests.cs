using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Backend.IntegrationTests.Helpers;
using Btms.Business.Commands;
using Btms.Model;
using Btms.Model.Auditing;
using Btms.SyncJob;
using FluentAssertions;
using TestGenerator.IntegrationTesting.Backend;
using TestGenerator.IntegrationTesting.Backend.Extensions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Btms.Backend.IntegrationTests;

[Trait("Category", "Integration")]
public class SmokeTests : BaseApiTests, IClassFixture<ApplicationFactory>
{
    private readonly JsonSerializerOptions jsonOptions;

    public SmokeTests(ApplicationFactory factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
        jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        jsonOptions.PropertyNameCaseInsensitive = true;
    }

    [Fact]
    public async Task CancelSyncJob()
    {
        //Arrange
        await base.ClearDb();
        var (response, jobId) = await Client.StartJob(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        }, "/sync/import-notifications");

        //Act
        var cancelJobResponse = await Client.CancelJob(jobId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        cancelJobResponse.IsSuccessStatusCode.Should().BeTrue(cancelJobResponse.StatusCode.ToString());


        // Check Api
        var jobResponse = await Client.GetJob(jobId);
        var syncJob = await jobResponse.Content.ReadFromJsonAsync<SyncJobResponse>(jsonOptions);
        syncJob?.Status.Should().Be(SyncJobStatus.Cancelled);
    }

    [Fact]
    public async Task SyncNotifications()
    {
        //Arrange
        await base.ClearDb();
        await Client.MakeSyncNotificationsRequest(new SyncNotificationsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        // Assert
        // Check Db
        Factory.GetDbContext().Notifications.Count().Should().Be(5);

        // Check Api
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/import-notifications");
        jsonClientResponse.Data.Count.Should().Be(5);

        // Check Audit Entry
        Client.GetFirstImportNotification()
            .AuditEntries
            .First(a =>
                a.Status == "Created")
            .Id
            .Should().StartWith($"SmokeTest{Path.DirectorySeparatorChar}");

    }

    // [Fact]
    // public void AuditEntryIdsShouldBeCorrectFormat()
    // {
    //     Client
    //         .GetSingleMovement()
    //         .AuditEntries
    //         .Where(a => a.CreatedBy is CreatedBySystem.Alvs or CreatedBySystem.Cds && a.Status == "Created" )
    //         .Should().AllSatisfy(
    //             a =>
    //             {
    //
    //                 a.Id.Should().StartWith("");
    //             });
    // }

    [Fact]
    public async Task SyncDecisions()
    {
        //Arrange 
        await base.ClearDb();
        await SyncClearanceRequests();
        await Client.MakeSyncDecisionsRequest(new SyncDecisionsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        // Assert
        var existingMovement = await Factory.GetDbContext().Movements.Find("CHEDPGB20241039875A5");

        existingMovement.Should().NotBeNull();
        existingMovement?.Items[0].Checks.Should().NotBeNull();
        existingMovement?.Items[0].Checks?.Length.Should().Be(1);
        existingMovement?.Items[0].Checks?[0].CheckCode.Should().Be("H234");
        existingMovement?.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");

        // Check Api
        var jsonClientResponse =
            Client.AsJsonApiClient().GetById("CHEDPGB20241039875A5", "api/movements");
        var movement = jsonClientResponse.GetResourceObject<Movement>();
        movement.Items[0].Checks?.Length.Should().Be(1);
        movement.Items[0].Checks?[0].CheckCode.Should().Be("H234");
        movement.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");
    }

    [Fact]
    public async Task SyncClearanceRequests()
    {
        //Arrange
        await base.ClearDb();

        //Act
        await Client.MakeSyncClearanceRequest(new SyncClearanceRequestsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        // Assert
        Factory.GetDbContext().Movements.Count().Should().Be(5);

        // Check Api
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/movements");
        jsonClientResponse.Data.Count.Should().Be(5);
    }

    [Fact]
    public async Task SyncGmrs()
    {
        //Arrange
        await base.ClearDb();

        //Act
        await Client.MakeSyncGmrsRequest(new SyncGmrsCommand
        {
            SyncPeriod = SyncPeriod.All,
            RootFolder = "SmokeTest"
        });

        // Assert
        Factory.GetDbContext().Gmrs.Count().Should().Be(3);

        // Check Api
        var jsonClientResponse = Client.AsJsonApiClient().Get("api/gmrs");
        jsonClientResponse.Data.Count.Should().Be(3);
    }
}
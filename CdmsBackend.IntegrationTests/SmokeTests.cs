using Cdms.Business.Commands;
using Cdms.Model;
using Cdms.SyncJob;
using CdmsBackend.IntegrationTests.Helpers;
using CdmsBackend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace CdmsBackend.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class SmokeTests : BaseApiTests, IClassFixture<IntegrationTestsApplicationFactory>
    {
        private readonly JsonSerializerOptions jsonOptions;

        public SmokeTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper) :base(factory, testOutputHelper)
        {
            jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.PropertyNameCaseInsensitive = true;
        }

        [Fact]
        public async Task CancelSyncJob()
        {
            //Arrange
            await IntegrationTestsApplicationFactory.ClearDb(Client);
            var jobId = await StartJob(new SyncNotificationsCommand()
            {
                SyncPeriod = SyncPeriod.All,
                RootFolder = "SmokeTest"
            }, "/sync/import-notifications");

            //Act
            var cancelJobResponse = await Client.GetAsync($"/sync/jobs/{jobId}/cancel");



            // Assert
            cancelJobResponse.IsSuccessStatusCode.Should().BeTrue(cancelJobResponse.StatusCode.ToString());
           

            // Check Api
            var jobResponse = await Client.GetAsync($"/sync/jobs/{jobId}");
            var syncJob = await jobResponse.Content.ReadFromJsonAsync<SyncJobResponse>(jsonOptions);
            syncJob?.Status.Should().Be(SyncJobStatus.Cancelled);
        }

        [Fact]
        public async Task SyncNotifications()
        {
            //Arrange
            await IntegrationTestsApplicationFactory.ClearDb(Client);
            await MakeSyncNotificationsRequest(new SyncNotificationsCommand()
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
        }

        [Fact]
        public async Task SyncDecisions()
        {
            //Arrange 
            await IntegrationTestsApplicationFactory.ClearDb(Client);
            await SyncClearanceRequests();
            await MakeSyncDecisionsRequest(new SyncDecisionsCommand()
            {
                SyncPeriod = SyncPeriod.All,
                RootFolder = "SmokeTest"
            });

            // Assert
            var existingMovement = await Factory.GetDbContext().Movements.Find("CHEDPGB20241039875A5");

            existingMovement.Should().NotBeNull();
            existingMovement.Items[0].Checks.Should().NotBeNull();
            existingMovement.Items[0].Checks?.Length.Should().Be(1);
            existingMovement.Items[0].Checks?[0].CheckCode.Should().Be("H234");
            existingMovement.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");

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
            await IntegrationTestsApplicationFactory.ClearDb(Client);

            //Act
            await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand()
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
            await IntegrationTestsApplicationFactory.ClearDb(Client);

            //Act
            await MakeSyncGmrsRequest(new SyncGmrsCommand()
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

        private async Task<string?> StartJob<T>(T command, string uri)
        {
            var jsonData = JsonSerializer.Serialize(command);
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //Act
            var response = await Client.PostAsync(uri, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            return Path.GetFileName(response.Headers.Location?.ToString());
        }
    }
}
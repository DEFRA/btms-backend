using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdms.Business.Commands;
using Cdms.Model;
using Cdms.SyncJob;
using CdmsBackend.IntegrationTests.Helpers;
using CdmsBackend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CdmsBackend.IntegrationTests
{
    

    [Trait("Category", "Integration")]
    public class SmokeTests :
        IClassFixture<IntegrationTestsApplicationFactory>
    {
        private readonly HttpClient client;
        private readonly IntegrationTestsApplicationFactory factory;

        public SmokeTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
        {
            this.factory = factory;
            this.factory.TestOutputHelper = testOutputHelper;
            this.factory.DatabaseName = "SmokeTests";
            client =
                this.factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [Fact]
        public async Task SyncNotifications()
        {
            //Arrange
            await IntegrationTestsApplicationFactory.ClearDb(client);
            await MakeSyncNotificationsRequest(new SyncNotificationsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            // Check Db
            factory.GetDbContext().Notifications.Count().Should().Be(5);

            // Check Api
            var jsonClientResponse = client.AsJsonApiClient().Get("api/import-notifications");
            jsonClientResponse.Data.Count.Should().Be(5);
        }

        [Fact]
        public async Task SyncDecisions()
        {
            //Arrange 
            await IntegrationTestsApplicationFactory.ClearDb(client);
            await SyncClearanceRequests();
            await MakeSyncDecisionsRequest(new SyncDecisionsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            var existingMovement = await factory.GetDbContext().Movements.Find("CHEDPGB20241039875A5");

            existingMovement.Should().NotBeNull();
            existingMovement.Items[0].Checks.Should().NotBeNull();
            existingMovement.Items[0].Checks?.Length.Should().Be(1);
            existingMovement.Items[0].Checks?[0].CheckCode.Should().Be("H234");
            existingMovement.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");

            // Check Api
            var jsonClientResponse =
                client.AsJsonApiClient().GetById("CHEDPGB20241039875A5", "api/movements");
            var movement = jsonClientResponse.GetResourceObject<Movement>();
            movement.Items[0].Checks?.Length.Should().Be(1);
            movement.Items[0].Checks?[0].CheckCode.Should().Be("H234");
            movement.Items[0].Checks?[0].DepartmentCode.Should().Be("PHA");
        }

        [Fact]
        public async Task SyncClearanceRequests()
        {
            //Arrange
            await IntegrationTestsApplicationFactory.ClearDb(client);

            //Act
            await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            factory.GetDbContext().Movements.Count().Should().Be(5);

            // Check Api
            var jsonClientResponse = client.AsJsonApiClient().Get("api/movements");
            jsonClientResponse.Data.Count.Should().Be(5);
        }

        [Fact]
        public async Task SyncGmrs()
        {
            //Arrange
            await IntegrationTestsApplicationFactory.ClearDb(client);

            //Act
            await MakeSyncGmrsRequest(new SyncGmrsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            factory.GetDbContext().Gmrs.Count().Should().Be(3);

            // Check Api
            var jsonClientResponse = client.AsJsonApiClient().Get("api/gmrs");
            jsonClientResponse.Data.Count.Should().Be(3);
        }

        private async Task WaitOnJobCompleting(Uri jobUri)
        {
            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new JsonStringEnumConverter());
            jsonOptions.PropertyNameCaseInsensitive = true;

            var jobStatusTask = Task.Run(async () =>
            {
                SyncJobStatus status = SyncJobStatus.Pending;

                while (status != SyncJobStatus.Completed)
                {
                    await Task.Delay(200);
                    var jobResponse = await client.GetAsync(jobUri);
                    var syncJob = await jobResponse.Content.ReadFromJsonAsync<SyncJobResponse>(jsonOptions);
                    status = syncJob!.Status;
                }
            });

            var winningTask = await Task.WhenAny(
                jobStatusTask,
                Task.Delay(TimeSpan.FromMinutes(1)));

            if (winningTask != jobStatusTask)
            {
                Assert.Fail("Waiting for job to complete timed out!");
            }

        }


        private Task<HttpResponseMessage> MakeSyncDecisionsRequest(SyncDecisionsCommand command)
        {
            return PostCommand(command, "/sync/decisions");
        }

        private Task<HttpResponseMessage> MakeSyncNotificationsRequest(SyncNotificationsCommand command)
        {
            return PostCommand(command, "/sync/import-notifications");
        }

        private Task<HttpResponseMessage> MakeSyncClearanceRequest(SyncClearanceRequestsCommand command)
        {
            return PostCommand(command, "/sync/clearance-requests");
        }

        private Task<HttpResponseMessage> MakeSyncGmrsRequest(SyncGmrsCommand command)
        {
            return PostCommand(command, "/sync/gmrs");

        }


        private async Task<HttpResponseMessage> PostCommand<T>(T command, string uri)
        {
            var jsonData = JsonSerializer.Serialize(command);
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //Act
            var response = await client.PostAsync(uri, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);

            //get job id and wait for job to be completed
            var jobUri = response.Headers.Location;
            await WaitOnJobCompleting(jobUri!);

            return response;
        }
    }
}
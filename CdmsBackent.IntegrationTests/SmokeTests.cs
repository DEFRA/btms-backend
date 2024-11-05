using System.Data.Common;
using System.Net;
using System.Text;
using System.Text.Json;
using Cdms.Backend.Data;
using Cdms.Business.Commands;
using Cdms.Model;
using Cdms.Model.Gvms;
using Cdms.Model.Ipaffs;
using CdmsBackend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;
using Xunit.Abstractions;

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
            this.factory.testOutputHelper = testOutputHelper;
            client =
                this.factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [Fact]
        public async Task SyncNotifications()
        {
            //Arrange
            await factory.ClearDb(client);
            var response = await MakeSyncNotificationsRequest(new SyncNotificationsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Check Db
            factory.GetDbContext().Notifications.Count().Should().Be(5);
            factory.GetDbContext().Inbox.Count().Should().Be(5);

            // Check Api
            var jsonClientResponse = client.AsJsonApiClient().Get<ImportNotification>("api/importnotifications");
            jsonClientResponse.IsSuccess.Should().BeTrue();
            jsonClientResponse.DocumentRoot.Data.Length.Should().Be(5);
        }

        [Fact]
        public async Task SyncDecisions()
        {
            //Arrange
            await factory.ClearDb(client);
            await SyncClearanceRequests();
            var response = await MakeSyncDecisionsRequest(new SyncDecisionsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var existingMovement = await factory.GetDbContext().Movements.Find("CHEDPGB20241039875A5");
            factory.GetDbContext().Inbox.Count().Should().Be(6);

            existingMovement.Should().NotBeNull();
            existingMovement.Items[0].Checks.Should().NotBeNull();
            existingMovement.Items[0].Checks.Length.Should().Be(1);
            existingMovement.Items[0].Checks[0].CheckCode.Should().Be("H234");
            existingMovement.Items[0].Checks[0].DepartmentCode.Should().Be("PHA");

            // Check Api
            var jsonClientResponse =
                client.AsJsonApiClient().GetById<Movement>("CHEDPGB20241039875A5", "api/movements");
            jsonClientResponse.IsSuccess.Should().BeTrue();
            jsonClientResponse.DocumentRoot.Data.Items[0].Checks.Length.Should().Be(1);
            jsonClientResponse.DocumentRoot.Data.Items[0].Checks[0].CheckCode.Should().Be("H234");
            jsonClientResponse.DocumentRoot.Data.Items[0].Checks[0].DepartmentCode.Should().Be("PHA");
        }

        [Fact]
        public async Task SyncClearanceRequests()
        {
            //Arrange
            await factory.ClearDb(client);

            //Act
            var response = await MakeSyncClearanceRequest(new SyncClearanceRequestsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            factory.GetDbContext().Movements.Count().Should().Be(5);
            factory.GetDbContext().Inbox.Count().Should().Be(5);

            // Check Api
            var jsonClientResponse = client.AsJsonApiClient().Get<Movement>("api/movements");
            jsonClientResponse.IsSuccess.Should().BeTrue();
            jsonClientResponse.DocumentRoot.Data.Length.Should().Be(5);
        }

        [Fact]
        public async Task SyncGmrs()
        {
            //Arrange
            await factory.ClearDb(client);

            //Act
            var response = await MakeSyncGmrsRequest(new SyncGmrsCommand()
            {
                SyncPeriod = SyncPeriod.All, RootFolder = "SmokeTest"
            });

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            factory.GetDbContext().Gmrs.Count().Should().Be(3);
            factory.GetDbContext().Inbox.Count().Should().Be(1);

            // Check Api
            var jsonClientResponse = client.AsJsonApiClient().Get<Gmr>("api/gmrs");
            jsonClientResponse.IsSuccess.Should().BeTrue();
            jsonClientResponse.DocumentRoot.Data.Length.Should().Be(3);
        }


        private Task<HttpResponseMessage> MakeSyncDecisionsRequest(SyncDecisionsCommand command)
        {
            return PostCommand(command, "/sync/decisions");
        }

        private Task<HttpResponseMessage> MakeSyncNotificationsRequest(SyncNotificationsCommand command)
        {
            return PostCommand(command, "/sync/notifications");
        }

        private Task<HttpResponseMessage> MakeSyncClearanceRequest(SyncClearanceRequestsCommand command)
        {
            return PostCommand(command, "/sync/clearance-requests");
        }

        private Task<HttpResponseMessage> MakeSyncGmrsRequest(SyncGmrsCommand command)
        {
            return PostCommand(command, "/sync/gmrs");
        }


        private Task<HttpResponseMessage> PostCommand<T>(T command, string uri)
        {
            var jsonData = JsonSerializer.Serialize(command);
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //Act
            return client.PostAsync(uri, content);
        }
    }
}
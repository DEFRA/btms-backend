using Cdms.Business.Commands;
using Cdms.Model;
using CdmsBackend.IntegrationTests.Helpers;
using CdmsBackend.IntegrationTests.JsonApiClient;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CdmsBackend.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class SmokeTests(IntegrationTestsApplicationFactory factory, ITestOutputHelper testOutputHelper)
        : BaseApiTests(factory, testOutputHelper), IClassFixture<IntegrationTestsApplicationFactory>
    {
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
    }
}
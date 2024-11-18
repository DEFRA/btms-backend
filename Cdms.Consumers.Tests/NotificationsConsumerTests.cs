using Cdms.Types.Ipaffs;
using Cdms.Types.Ipaffs.Mapping;
using FluentAssertions;
using SlimMessageBus.Host;
using TestDataGenerator;
using Xunit;

namespace Cdms.Consumers.Tests
{
    public class NotificationsConsumerTests : ConsumerTests
    {
        [Fact]
        public async Task WhenNotificationNotExists_ThenShouldBeCreated()
        {
            // ARRANGE
            var notification = CreateImportNotification();
            var dbContext = CreateDbContext();


            var consumer = new NotificationConsumer(dbContext);
            consumer.Context = new ConsumerContext()
            {
                Headers = new Dictionary<string, object>() { { "messageId", notification!.ReferenceNumber! } }
            };

            // ACT
            await consumer.OnHandle(notification);

            // ASSERT
            var savedNotification = await dbContext.Notifications.Find(notification!.ReferenceNumber!);
            savedNotification.Should().NotBeNull();
            savedNotification.AuditEntries.Count.Should().Be(1);
            savedNotification.AuditEntries[0].Status.Should().Be("Created");
        }

        [Fact]
        public async Task WhenNotificationExists_AndLastUpdatedIsNewer_ThenShouldBeUpdated()
        {
            // ARRANGE
            var notification = CreateImportNotification();
            var dbContext = CreateDbContext();
            await dbContext.Notifications.Insert(notification.MapWithTransform());
            notification.LastUpdated = notification?.LastUpdated?.AddHours(1);


            var consumer = new NotificationConsumer(dbContext);
            consumer.Context = new ConsumerContext()
            {
                Headers = new Dictionary<string, object>() { { "messageId", notification!.ReferenceNumber! } }
            };

            // ACT
            await consumer.OnHandle(notification);

            // ASSERT
            var savedNotification = await dbContext.Notifications.Find(notification!.ReferenceNumber!);
            savedNotification.Should().NotBeNull();
            savedNotification.AuditEntries.Count.Should().Be(1);
            savedNotification.AuditEntries[0].Status.Should().Be("Updated");
        }

        private ImportNotification CreateImportNotification()
        {
            return ImportNotificationBuilder.Default()
                .WithReferenceNumber(ImportNotificationTypeEnum.Chedpp, 1, DateTime.UtcNow, 1)
                .WithRandomCommodities(1, 2)
                .Do(x =>
                {
                    foreach (var parameterSet in x.PartOne?.Commodities?.ComplementParameterSets!)
                    {
                        parameterSet.KeyDataPairs = null;
                    }
                }).Build();
        }
    }
}
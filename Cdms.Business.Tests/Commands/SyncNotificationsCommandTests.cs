using Cdms.BlobService;
using Cdms.Business.Commands;
using Cdms.Model.Extensions;
using Cdms.SensitiveData;
using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SlimMessageBus;
using TestDataGenerator;
using Xunit;

namespace Cdms.Business.Tests.Commands
{
    public class SyncNotificationsCommandTests
    {
        [Fact]
        public async Task WhenNotificationBlobsExist_ThenTheyShouldBePlacedOnInternalBus()
        {
            // ARRANGE
            var notification = CreateImportNotification();
            var command = new SyncNotificationsCommand();

            var bus = Substitute.For<IPublishBus>();
            var blob = Substitute.For<IBlobService>();
            blob.GetResourcesAsync(Arg.Any<string>())
                .Returns(
                    new TestBlobItem(notification.ReferenceNumber, notification.ToJsonString()).ToAsyncEnumerator());


            var handler = new SyncNotificationsCommand.Handler(
                bus,
                NullLogger<SyncNotificationsCommand>.Instance,
                new SensitiveDataSerializer(Options.Create(SensitiveDataOptions.WithSensitiveData)),
                blob);

            // ACT
            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            bus.Received(4).Publish(Arg.Any<ImportNotification>(), "NOTIFICATIONS",
                Arg.Any<IDictionary<string, object>>());
        }

        private ImportNotification CreateImportNotification()
        {
            return ImportNotificationBuilder.Default()
                .Do(x =>
                {
                    foreach (var parameterSet in x.PartOne.Commodities.ComplementParameterSets)
                    {
                        parameterSet.KeyDataPairs = null;
                    }
                }).Build();
        }
    }
}
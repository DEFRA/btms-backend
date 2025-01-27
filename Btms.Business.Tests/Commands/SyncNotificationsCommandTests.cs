using Btms.BlobService;
using Btms.Business.Commands;
using Btms.Metrics;
using Btms.Model.Extensions;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SlimMessageBus;
using TestDataGenerator;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Business.Tests.Commands;

public class SyncNotificationsCommandTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task WhenNotificationBlobsExist_ThenTheyShouldBePlacedOnInternalBus()
    {
        // ARRANGE
        var notification = CreateImportNotification();
        var command = new SyncNotificationsCommand();
        var jobStore = new SyncJobStore(NullLogger<SyncJobStore>.Instance);
        jobStore.CreateJob(command.JobId, SyncPeriod.All.ToString(), "Test Job");

        var bus = Substitute.For<IPublishBus>();
        var blob = Substitute.For<IBlobService>();
        blob.GetResourcesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new TestBlobItem(notification.ReferenceNumber!, notification.ToJsonString()).ToAsyncEnumerator());

        blob.GetResource(Arg.Any<IBlobItem>(), Arg.Any<CancellationToken>())
            .Returns(notification.ToJsonString());


        var handler = new SyncNotificationsCommand.Handler(
                
            new SyncMetrics(new DummyMeterFactory()),
            bus,
            TestLogger.Create<SyncNotificationsCommand>(outputHelper),
            new SensitiveDataSerializer(Options.Create(SensitiveDataOptions.WithSensitiveData), NullLogger<SensitiveDataSerializer>.Instance),
            blob,
            Options.Create(new BusinessOptions()),
            jobStore);

        // ACT
        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await bus.Received(4).Publish(Arg.Any<ImportNotification>(), "NOTIFICATIONS",
            Arg.Any<IDictionary<string, object>>(), Arg.Any<CancellationToken>());
    }

    private static ImportNotification CreateImportNotification()
    {
        return ImportNotificationBuilder.Default()
            .Do(x =>
            {
                foreach (var parameterSet in x.PartOne!.Commodities!.ComplementParameterSets!)
                {
                    parameterSet.KeyDataPairs = null;
                }
            }).Build();
    }
}
using Btms.BlobService;
using Btms.Business.Commands;
using Btms.Metrics;
using Btms.Model.Extensions;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SlimMessageBus;
using TestDataGenerator;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Business.Tests.Commands;

public class SyncClearanceRequestsCommandTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task WhenClearanceRequestBlobsExist_ThenTheyShouldBePlacedOnInternalBus()
    {
        var clearanceRequest = ClearanceRequestBuilder.Default().Build();
        var command = new SyncClearanceRequestsCommand();
        var jobStore = new SyncJobStore(NullLogger<SyncJobStore>.Instance);
        jobStore.CreateJob(command.JobId, SyncPeriod.All.ToString(), "ClearanceRequests");


        var bus = Substitute.For<IPublishBus>();
        var blob = Substitute.For<IBlobService>();
        blob.GetResourcesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new TestBlobItem(clearanceRequest.Header!.EntryReference!, clearanceRequest.ToJsonString())
                    .ToAsyncEnumerator());

        blob.GetResource(Arg.Any<IBlobItem>(), Arg.Any<CancellationToken>())
            .Returns(clearanceRequest.ToJsonString());


        var handler = new SyncClearanceRequestsCommand.Handler(
            new SyncMetrics(new DummyMeterFactory()),
            bus,
            TestLogger.Create<SyncClearanceRequestsCommand>(outputHelper),
            new SensitiveDataSerializer(Options.Create(SensitiveDataOptions.WithSensitiveData), NullLogger<SensitiveDataSerializer>.Instance, new SensitiveFieldsProvider()),
            blob,
            Options.Create(new BusinessOptions()),
            jobStore);

        await handler.Handle(command, CancellationToken.None);

        // ASSERT
        await bus.Received(1).Publish(Arg.Any<AlvsClearanceRequest>(), "CLEARANCEREQUESTS",
            Arg.Any<IDictionary<string, object>>(), Arg.Any<CancellationToken>());
    }
}
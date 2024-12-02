using Cdms.BlobService;
using Cdms.Business.Commands;
using Cdms.Metrics;
using Cdms.Model.Extensions;
using Cdms.SensitiveData;
using Cdms.SyncJob;
using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SlimMessageBus;
using TestDataGenerator;
using Xunit;
using Xunit.Abstractions;

namespace Cdms.Business.Tests.Commands
{
    public class SyncDecisionsCommandTests(ITestOutputHelper outputHelper)
    {
        [Fact]
        public async Task WhenDecisionBlobsExist_ThenTheyShouldBePlacedOnInternalBus()
        {
            var clearanceRequest = ClearanceRequestBuilder.Default().Build();
            var command = new SyncDecisionsCommand();
            var jobStore = new SyncJobStore();
            jobStore.CreateJob(command.JobId, SyncPeriod.All.ToString(), "Test Job");

            var bus = Substitute.For<IPublishBus>();
            var blob = Substitute.For<IBlobService>();
            blob.GetResourcesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(
                    new TestBlobItem(clearanceRequest!.Header!.EntryReference!, clearanceRequest.ToJsonString())
                        .ToAsyncEnumerator());

            blob.GetResource(Arg.Any<IBlobItem>(), Arg.Any<CancellationToken>())
                .Returns(clearanceRequest.ToJsonString());


            var handler = new SyncDecisionsCommand.Handler(
                new SyncMetrics(new DummyMeterFactory()),
                bus,
                TestLogger.Create<SyncDecisionsCommand>(outputHelper),
                new SensitiveDataSerializer(Options.Create(SensitiveDataOptions.WithSensitiveData), NullLogger<SensitiveDataSerializer>.Instance),
                blob,
                Options.Create(new BusinessOptions()),
                jobStore);

            await handler.Handle(command, CancellationToken.None);

            // ASSERT
            await bus.Received(1).Publish(Arg.Any<AlvsClearanceRequest>(), "DECISIONS",
                Arg.Any<IDictionary<string, object>>(), Arg.Any<CancellationToken>());
        }
    }
}
using Cdms.BlobService;
using Cdms.SensitiveData;
using Cdms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Cdms.Business.Commands;

public class SyncClearanceRequestsCommand(BusinessOptions options) : SyncCommand(options)
{
    internal class Handler(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<SyncClearanceRequestsCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService)
        : SyncCommand.Handler<SyncClearanceRequestsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer,
            blobService)
    {
        public override async Task Handle(SyncClearanceRequestsCommand request, CancellationToken cancellationToken)
        {
            await SyncBlobPaths<AlvsClearanceRequest>(request.SyncPeriod, "ALVS", $"{request.RootFolder}/ALVS");
        }
    }
}
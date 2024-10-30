using Cdms.BlobService;
using Cdms.SensitiveData;
using Cdms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Cdms.Business.Commands;

public class SyncDecisionsCommand : SyncCommand
{
    internal class Handler(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<SyncDecisionsCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService)
        : SyncCommand.Handler<SyncDecisionsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService)
    {
        public override async Task Handle(SyncDecisionsCommand request, CancellationToken cancellationToken)
        {
            await SyncBlobPaths<AlvsClearanceRequest>(request.SyncPeriod, "DECISIONS", "RAW/DECISONS");
        }
    }
}
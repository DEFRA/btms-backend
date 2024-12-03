using Btms.BlobService;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Btms.Business.Commands;

public class SyncDecisionsCommand : SyncCommand
{
    internal class Handler(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<SyncDecisionsCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService,
        IOptions<BusinessOptions> businessOptions,
        ISyncJobStore syncJobStore)
        : SyncCommand.Handler<SyncDecisionsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService, syncJobStore)
    {
        public override async Task Handle(SyncDecisionsCommand request, CancellationToken cancellationToken)
        {
            var rootFolder = string.IsNullOrEmpty(request.RootFolder)
                ? businessOptions.Value.DmpBlobRootFolder
                : request.RootFolder;
            await SyncBlobPaths<AlvsClearanceRequest>(request.SyncPeriod, "DECISIONS", request.JobId,
                cancellationToken,$"{rootFolder}/DECISIONS");
        }
    }

    public override string Resource => "Decision";
}
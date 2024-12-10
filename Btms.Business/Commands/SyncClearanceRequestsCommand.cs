using Btms.BlobService;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Btms.Business.Commands;

public class SyncClearanceRequestsCommand : SyncCommand
{
    internal class Handler(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<SyncClearanceRequestsCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService,
        IOptions<BusinessOptions> businessOptions,
        ISyncJobStore syncJobStore)
        : Handler<SyncClearanceRequestsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer,
            blobService, syncJobStore)
    {
        public override async Task Handle(SyncClearanceRequestsCommand request, CancellationToken cancellationToken)
        {
            var rootFolder = string.IsNullOrEmpty(request.RootFolder)
                ? businessOptions.Value.DmpBlobRootFolder
                : request.RootFolder;
            await SyncBlobPaths<AlvsClearanceRequest>(request.SyncPeriod, "ALVS", request.JobId, cancellationToken,$"{rootFolder}/ALVS");
        }
    }

    public override string Resource => "ClearanceRequest";
}
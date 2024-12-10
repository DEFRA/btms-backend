using Btms.BlobService;
using Btms.Metrics;
using Btms.SensitiveData;
using Btms.SyncJob;
using Btms.Types.Gvms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Btms.Business.Commands;

public class SyncGmrsCommand : SyncCommand
{
    internal class Handler(
        SyncMetrics syncMetrics,
        IPublishBus bus,
        ILogger<SyncGmrsCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService,
        IOptions<BusinessOptions> businessOptions,
        ISyncJobStore syncJobStore)
        : Handler<SyncGmrsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService, businessOptions, syncJobStore)
    {
        public override async Task Handle(SyncGmrsCommand request, CancellationToken cancellationToken)
        {
            var rootFolder = string.IsNullOrEmpty(request.RootFolder)
                ? Options.DmpBlobRootFolder
                : request.RootFolder;
            await SyncBlobPaths<SearchGmrsForDeclarationIdsResponse>(request.SyncPeriod, "GMR", request.JobId,
                cancellationToken, $"{rootFolder}/GVMSAPIRESPONSE");
        }
    }

    public override string Resource => "Gmr";
}
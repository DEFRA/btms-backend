using Cdms.BlobService;
using Cdms.SensitiveData;
using Cdms.SyncJob;
using Cdms.Types.Gvms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Cdms.Business.Commands;

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
        : SyncCommand.Handler<SyncGmrsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer, blobService, syncJobStore)
    {
        public override async Task Handle(SyncGmrsCommand request, CancellationToken cancellationToken)
        {
            var rootFolder = string.IsNullOrEmpty(request.RootFolder)
                ? businessOptions.Value.DmpBlobRootFolder
                : request.RootFolder;
            await SyncBlobPaths<SearchGmrsForDeclarationIdsResponse>(request.SyncPeriod, "GMR", request.JobId,
                $"{rootFolder}/GVMSAPIRESPONSE");
        }
    }
}
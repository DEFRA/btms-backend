using Cdms.BlobService;
using Cdms.SensitiveData;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Cdms.Business.Commands
{
    public class SyncNotificationsCommand : SyncCommand
    {
        internal class Handler(
            SyncMetrics syncMetrics,
            IPublishBus bus,
            ILogger<SyncNotificationsCommand> logger,
            ISensitiveDataSerializer sensitiveDataSerializer,
            IBlobService blobService,
            IOptions<BusinessOptions> businessOptions)
            : SyncCommand.Handler<SyncNotificationsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer,
                blobService)
        {
            public override async Task Handle(SyncNotificationsCommand request, CancellationToken cancellationToken)
            {
                var rootFolder = string.IsNullOrEmpty(request.RootFolder)
                    ? businessOptions.Value.DmpBlobRootFolder
                    : request.RootFolder;
                await SyncBlobPaths<Cdms.Types.Ipaffs.ImportNotification>(request.SyncPeriod, "NOTIFICATIONS",
                    $"{rootFolder}/IPAFFS/CHEDA",
                    $"{rootFolder}/IPAFFS/CHEDD",
                    $"{rootFolder}/IPAFFS/CHEDP",
                    $"{rootFolder}/IPAFFS/CHEDPP");
            }
        }
    }
}
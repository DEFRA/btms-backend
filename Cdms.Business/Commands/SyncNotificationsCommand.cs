using Cdms.BlobService;
using Cdms.SensitiveData;
using Microsoft.Extensions.Logging;
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
            IBlobService blobService)
            : SyncCommand.Handler<SyncNotificationsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer,
                blobService)
        {
            public override async Task Handle(SyncNotificationsCommand request, CancellationToken cancellationToken)
            {
                await SyncBlobPaths<Cdms.Types.Ipaffs.ImportNotification>(request.SyncPeriod, "NOTIFICATIONS",
                    "RAW/IPAFFS/CHEDA",
                    "RAW/IPAFFS/CHEDD",
                    "RAW/IPAFFS/CHEDP",
                    "RAW/IPAFFS/CHEDPP");
            }
        }
    }
}
using Azure.Core;
using Cdms.BlobService;
using Cdms.SensitiveData;
using Cdms.SyncJob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlimMessageBus;

namespace Cdms.Business.Commands
{
    public class SyncNotificationsCommand : SyncCommand
    {
        public string[] ChedTypes { get; set; } = [];
        public override string Resource => "ImportNotification";

        public string[] BlobFiles { get; set; } = [];


        internal class Handler(
            SyncMetrics syncMetrics,
            IPublishBus bus,
            ILogger<SyncNotificationsCommand> logger,
            ISensitiveDataSerializer sensitiveDataSerializer,
            IBlobService blobService,
            IOptions<BusinessOptions> businessOptions,
            ISyncJobStore syncJobStore)
            : SyncCommand.Handler<SyncNotificationsCommand>(syncMetrics, bus, logger, sensitiveDataSerializer,
                blobService, syncJobStore)
        {
            public override async Task Handle(SyncNotificationsCommand request, CancellationToken cancellationToken)
            {
                var rootFolder = string.IsNullOrEmpty(request.RootFolder)
                    ? businessOptions.Value.DmpBlobRootFolder
                    : request.RootFolder;

                if (request.BlobFiles.Any())
                {
                    await SyncBlobs<Types.Ipaffs.ImportNotification>(request.SyncPeriod, "NOTIFICATIONS",
                        request.JobId,
                        request.BlobFiles.Select(x => $"{rootFolder}/IPAFFS/{x}").ToArray());
                    return;
                }

                var chedTypesToSync = new List<string>();

                AddIf(chedTypesToSync, request, rootFolder, "CHEDA");
                AddIf(chedTypesToSync, request, rootFolder, "CHEDD");
                AddIf(chedTypesToSync, request, rootFolder, "CHEDP");
                AddIf(chedTypesToSync, request, rootFolder, "CHEDPP");

                await SyncBlobPaths<Types.Ipaffs.ImportNotification>(request.SyncPeriod, "NOTIFICATIONS",
                    request.JobId,
                    chedTypesToSync.ToArray());
            }

            private static void AddIf(List<string> chedTypesToSync, SyncNotificationsCommand request, string rootFolder, string chedType)
            {
                if (!request.ChedTypes.Any() || request.ChedTypes.Contains(chedType))
                {
                    chedTypesToSync.Add($"{rootFolder}/IPAFFS/{chedType}");
                }
            }
        }
    }
}
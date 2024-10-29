using Cdms.BlobService;
using Cdms.SensitiveData;
using Cdms.Types.Gmr;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Cdms.Business.Commands;

public class SyncGmrsCommand : SyncCommand
{
    internal class Handler(
        IPublishBus bus,
        ILogger<SyncGmrsCommand> logger,
        ISensitiveDataSerializer sensitiveDataSerializer,
        IBlobService blobService)
        : SyncCommand.Handler<SyncGmrsCommand>(bus, logger, sensitiveDataSerializer, blobService)
    {
        public override async Task Handle(SyncGmrsCommand request, CancellationToken cancellationToken)
        {
            await SyncBlobPaths<SearchGmrsForDeclarationIdsResponse>(request.SyncPeriod, "GMR", "RAW/GVMSAPIRESPONSE");
        }
    }
}
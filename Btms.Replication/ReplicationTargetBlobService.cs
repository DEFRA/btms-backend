using Btms.BlobService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.Replication;

public class ReplicationTargetBlobService(IServiceProvider serviceProvider,
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<ReplicationTargetBlobService> logger,
    IOptions<ReplicationOptions> options,
    IHttpClientFactory clientFactory) : BlobService.BlobService(serviceProvider, blobServiceClientFactory, logger, options, clientFactory)
{

}
using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.Replication;

public class ReplicationTargetBlobService(IServiceProvider serviceProvider,
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<ReplicationTargetBlobService> logger,
    IOptions<ReplicationOptions> options,
    IHttpClientFactory clientFactory) : BaseBlobService(serviceProvider, blobServiceClientFactory, logger, options, clientFactory)
{
    public async Task WriteResource(string path, string content, CancellationToken cancellationToken)
    {
        var client = CreateBlobClient(options.Value.Timeout, options.Value.Retries);
        var blobClient = client.GetBlobClient(path);

        var result = await blobClient.UploadAsync(BinaryData.FromString(content), cancellationToken);

        if (!result.HasValue())
        {
            throw new InvalidOperationException("Upload Failed");
        }
    }
}
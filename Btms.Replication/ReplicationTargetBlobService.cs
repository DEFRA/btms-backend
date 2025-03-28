using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.Replication;

public class ReplicationTargetBlobService(
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<ReplicationTargetBlobService> logger,
    IOptions<ReplicationOptions> options) : BaseBlobService(blobServiceClientFactory, logger, options)
{
    public async Task WriteResource(string path, string content, CancellationToken cancellationToken)
    {
        var client = CreateBlobClient();
        var blobClient = client.GetBlobClient(path);

        var result = await blobClient.UploadAsync(BinaryData.FromString(content), cancellationToken);

        if (!result.HasValue())
        {
            throw new InvalidOperationException("Upload Failed");
        }
    }
}
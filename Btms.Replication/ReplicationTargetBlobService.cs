using Btms.BlobService;
using Btms.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Btms.Replication;

public class ReplicationTargetBlobService(IServiceProvider serviceProvider,
    IBlobServiceClientFactory blobServiceClientFactory,
    ILogger<ReplicationTargetBlobService> logger,
    IOptions<ReplicationOptions> options,
    IHttpClientFactory clientFactory) : BaseBlobService(blobServiceClientFactory, logger, options)
{
    public async Task WriteResource(string path, string content, bool overwrite, CancellationToken cancellationToken)
    {
        var client = CreateBlobClient();
        var blobClient = client.GetBlobClient(path);

        var result = await blobClient.UploadAsync(BinaryData.FromString(content),
            overwrite: overwrite, cancellationToken: cancellationToken);

        if (!result.HasValue())
        {
            throw new InvalidOperationException("Upload Failed");
        }
    }
}
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Btms.BlobService;

public interface IBlobServiceClientFactory
{
    BlobServiceClient CreateBlobServiceClient(int timeout = default, int retries = default);
    BlobServiceClient CreateBlobServiceClient(IOptions<IBlobServiceOptions> options, int timeout = default, int retries = default);
}
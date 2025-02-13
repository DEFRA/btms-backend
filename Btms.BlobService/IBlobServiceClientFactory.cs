using Azure.Storage.Blobs;

namespace Btms.BlobService;

public interface IBlobServiceClientFactory
{
    BlobServiceClient CreateBlobServiceClient(int timeout = default, int retries = default);
}
using Azure.Storage.Blobs;

namespace Cdms.BlobService;

public interface IBlobServiceClientFactory
{
    BlobServiceClient CreateBlobServiceClient(int timeout = default, int retries = default);
}
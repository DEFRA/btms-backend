using Btms.Azure;

namespace Btms.BlobService;

public interface IBlobServiceOptions : IAzureConfig
{
    public string DmpBlobUri { get; set; }
    string DmpBlobContainer { get; set; }

    public int Retries { get; set; }
    public int Timeout { get; set; }
}
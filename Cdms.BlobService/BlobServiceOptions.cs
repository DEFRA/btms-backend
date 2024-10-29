using System.ComponentModel.DataAnnotations;
using Cdms.Azure;

namespace Cdms.BlobService;

public class BlobServiceOptions : IAzureConfig
{
    public const string SectionName = nameof(BlobServiceOptions);

    [Required] public string DmpBlobContainer { get; set; }

    [Required] public string DmpBlobUri { get; set; }
    public string AzureClientId { get; set; }
    public string AzureTenantId { get; set; }
    public string AzureClientSecret { get; set; }

    public int Retries { get; set; } = 3;

    public int Timeout { get; set; } = 10;
}
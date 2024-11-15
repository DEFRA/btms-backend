using System.ComponentModel.DataAnnotations;
using Cdms.Azure;
using Cdms.Common.Extensions;

namespace Cdms.BlobService;

public class BlobServiceOptions : IAzureConfig
{
    public const string SectionName = nameof(BlobServiceOptions);

    [Required] public string DmpBlobContainer { get; set; }

    [Required] public string DmpBlobUri { get; set; }
    [Required] public string AzureClientId { get; set; }
    [Required] public string AzureTenantId { get; set; }
    [Required] public string AzureClientSecret { get; set; }

    public int Retries { get; set; } = 3;

    public int Timeout { get; set; } = 10;
    
}
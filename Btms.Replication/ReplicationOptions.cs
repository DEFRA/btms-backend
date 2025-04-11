using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Btms.Azure;
using Btms.BlobService;
using Btms.Common.Options;

namespace Btms.Replication;

public class ReplicationOptions : IBlobServiceOptions
{
    public const string SectionName = nameof(ReplicationOptions);

    public bool Enabled { get; set; } = false;

    [RequiredIf(nameof(Enabled), true)]
    public string DmpBlobContainer { get; set; } = null!;

    [RequiredIf(nameof(Enabled), true)]
    public string DmpBlobUri { get; set; } = null!;

    [RequiredIf(nameof(Enabled), true)]
    public string AzureClientId { get; set; } = null!;

    [RequiredIf(nameof(Enabled), true)]
    public string AzureTenantId { get; set; } = null!;

    [RequiredIf(nameof(Enabled), true)]
    public string AzureClientSecret { get; set; } = null!;

    public int Retries { get; set; } = 3;
    public int Timeout { get; set; } = 10;
}
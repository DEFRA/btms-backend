using Btms.Azure;
using Btms.BlobService;

namespace Btms.Replication;

public class ReplicationOptions : BlobServiceOptions //IBlobServiceOptions
{
    public new const string SectionName = nameof(ReplicationOptions);
    //
    // public string DmpBlobUri { get; set; }
    // public string DmpBlobContainer { get; set; }
    //
    // public string? AzureClientId { get; set; }
    // public string? AzureTenantId { get; set; }
    // public string? AzureClientSecret { get; set; }

    public bool Enabled { get; set; } = false;

}
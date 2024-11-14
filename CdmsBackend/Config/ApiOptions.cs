using System.ComponentModel.DataAnnotations;
using Cdms.SensitiveData;
using Cdms.Common.Extensions;

namespace CdmsBackend.Config;

public class ApiOptions : IValidatingOptions
{
    public static readonly string SectionName = nameof(ApiOptions);

    public bool EnableManagement { get; set; } = default!;

    [ConfigurationKeyName("CDP_HTTPS_PROXY")]
    public string? CdpHttpsProxy { get; set; }

    // This is used by the azure library when connecting to auth related services
    // when connecting to blob storage
    [ConfigurationKeyName("HTTPS_PROXY")]
    public string? HttpsProxy { get; set; }

    public bool Validate()
    {
        return !CdpHttpsProxy.HasValue() || HttpsProxy.HasValue();
    }
}
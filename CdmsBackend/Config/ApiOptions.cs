using Cdms.SensitiveData;

namespace CdmsBackend.Config;

public class ApiOptions
{
    public const string SectionName = nameof(ApiOptions);

    public bool EnableManagement { get; set; } = default!;
}
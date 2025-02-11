using Btms.Common.Extensions;
using Microsoft.Extensions.Options;

namespace Btms.Backend.Config;

public class ApiOptions
{
    public static readonly string SectionName = nameof(ApiOptions);

    public bool EnableManagement { get; set; } = default!;

    public string? AnalyticsCachePolicy { get; set; } = default!;
    public bool EnableSync { get; set; } = true;

    [ConfigurationKeyName("CDP_HTTPS_PROXY")]
    public string? CdpHttpsProxy { get; set; }

    // This is used by the azure library when connecting to auth related services
    // when connecting to blob storage
    [ConfigurationKeyName("HTTPS_PROXY")]
    // public string? HttpsProxy { get; set; }
    public string? HttpsProxy => CdpHttpsProxy?.Contains("://") == true ? CdpHttpsProxy[(CdpHttpsProxy.IndexOf("://", StringComparison.Ordinal) + 3)..] : null;

    public Dictionary<string, string?> Credentials { get; set; } = [];

    public class Validator : IValidateOptions<ApiOptions>
    {
        /// <summary>
        /// Validates that CDP_HTTPS_PROXY is shaped correctly if present
        /// I'm sure this can be written more concisely, there are tests
        /// </summary>
        /// <returns></returns>
        public ValidateOptionsResult Validate(string? name, ApiOptions options)
        {
            if (options.CdpHttpsProxy.HasValue())
            {
                var valid = options.CdpHttpsProxy!.StartsWith("http");

                if (!valid)
                {
                    return ValidateOptionsResult.Fail("If CDP_HTTPS_PROXY is set, it must start with protocol");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}
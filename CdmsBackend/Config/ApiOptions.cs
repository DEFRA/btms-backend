using System.ComponentModel.DataAnnotations;
using Cdms.SensitiveData;
using Cdms.Common.Extensions;
using Microsoft.Extensions.Options;

namespace CdmsBackend.Config;

public class ApiOptions
{
    public static readonly string SectionName = nameof(ApiOptions);

    public bool EnableManagement { get; set; } = default!;

    [ConfigurationKeyName("CDP_HTTPS_PROXY")]
    public string? CdpHttpsProxy { get; set; }

    // This is used by the azure library when connecting to auth related services
    // when connecting to blob storage
    [ConfigurationKeyName("HTTPS_PROXY")]
    public string? HttpsProxy { get; set; }

    public class Validator() : IValidateOptions<ApiOptions>
    { 
        /// <summary>
        /// Validates that if we have CDP_HTTPS_PROXY we also have HTTPS_PROXY
        /// I'm sure this can be written more concisely, there are tests
        /// </summary>
        /// <returns></returns>
        public ValidateOptionsResult Validate(string? name, ApiOptions options)
        {
            var valid = !options.CdpHttpsProxy.HasValue() || options.HttpsProxy.HasValue();

            if (!valid)
            {
                return ValidateOptionsResult.Fail("If CDP_HTTPS_PROXY is set HTTPS_PROXY must also be set.");
            }
            if (options.CdpHttpsProxy.HasValue())
            {
                valid = options.CdpHttpsProxy!.StartsWith("http");
            
                if (!valid)
                {
                    return ValidateOptionsResult.Fail("If CDP_HTTPS_PROXY is set, it must start with protocol");
                }
            }
        
            if (options.HttpsProxy.HasValue())
            {
                valid = !options.HttpsProxy!.StartsWith("http");
            
                if (!valid)
                {
                    return ValidateOptionsResult.Fail("If HTTPS_PROXY is set HTTPS_PROXY it must not start with protocol");
                }
            }

            return ValidateOptionsResult.Success;
        }
    }
}
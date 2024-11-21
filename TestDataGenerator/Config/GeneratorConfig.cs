using Microsoft.Extensions.Configuration;
using TestDataGenerator.Services;

namespace TestDataGenerator.Config;

public enum StorageService
{
    AzureBlob,
    Local
}

public class GeneratorConfig : IAzureConfig
{
    public GeneratorConfig(IConfiguration configuration)
    {
        var dmpSlot = configuration["DMP_SLOT"]!;

        DmpEnvironment = configuration["DMP_ENVIRONMENT"]!;

        AzureClientId = configuration["AZURE_CLIENT_ID"];
        AzureTenantId = configuration["AZURE_TENANT_ID"];
        AzureClientSecret = configuration["AZURE_CLIENT_SECRET"];

        StorageService = Enum.TryParse<StorageService>(configuration["STORAGE_SERVICE"], true, out var tmp)
            ? tmp
            : StorageService.AzureBlob;

        DmpBlobUri = $"https://{configuration["DMP_BLOB_STORAGE_NAME"]!}.blob.core.windows.net";
        DmpBlobContainer = $"dmp-data-{dmpSlot}";
    }

    public string DmpEnvironment { get; set; } = default!;
    public string DmpBlobUri { get; set; } = default!;
    public string DmpBlobContainer { get; set; } = default!;

    public StorageService StorageService { get; set; }
    public string? AzureClientId { get; set; }
    public string? AzureTenantId { get; set; }
    public string? AzureClientSecret { get; set; }
}
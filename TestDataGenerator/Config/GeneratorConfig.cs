using Btms.Azure;
using Microsoft.Extensions.Configuration;

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

    public string DmpEnvironment { get; }
    public string DmpBlobUri { get; }
    public string DmpBlobContainer { get; }
    public StorageService StorageService { get; }
    public string? AzureClientId { get; }
    public string? AzureTenantId { get; }
    public string? AzureClientSecret { get; }
}
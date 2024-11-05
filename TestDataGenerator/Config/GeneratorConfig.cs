
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
    public string DmpEnvironment { get; set; } = default!;
    // public string DmpBusNamespace { get; set; } = default!;
    // public string DmpBusTopic { get; set; } = default!;
    // public string DmpBusSubscription { get; set; } = default!;
    //
    // public string? DmpBusPrivateEndpoint { get; set; } = default!;
    public string DmpBlobUri { get; set; } = default!;
    
    // public string? DmpBlobPrivateEndpoint { get; set; } = default!;
    public string DmpBlobContainer { get; set; } = default!;
    public string? AzureClientId { get; set; } = default!;
    public string? AzureTenantId { get; set; } = default!;
    public string? AzureClientSecret { get; set; } = default!;
    
    public StorageService StorageService { get; set; } = default!;
    
    public GeneratorConfig(IConfiguration configuration)
    {
        // CdsHttpsProxy = configuration["CDP_HTTPS_PROXY"];
        var dmpSlot = configuration["DMP_SLOT"]!;
        
        DmpEnvironment = configuration["DMP_ENVIRONMENT"]!;
        
        AzureClientId = configuration["AZURE_CLIENT_ID"];
        AzureTenantId = configuration["AZURE_TENANT_ID"];
        AzureClientSecret = configuration["AZURE_CLIENT_SECRET"];

        // StorageService tmp;
        this.StorageService = Enum.TryParse<StorageService>(configuration["STORAGE_SERVICE"], true, out var tmp) ? tmp : StorageService.AzureBlob;
        
        // DmpBusNamespace = $"{configuration["DMP_SERVICE_BUS_NAME"]!}.servicebus.windows.net";
        // DmpBusTopic = $"defra.trade.dmp.ingestipaffs.{DmpEnvironment}.{dmpSlot}.topic";
        // DmpBusSubscription = $"defra.trade.dmp.{DmpEnvironment}.{dmpSlot}.subscription";
        DmpBlobUri = $"https://{configuration["DMP_BLOB_STORAGE_NAME"]!}.blob.core.windows.net";
        DmpBlobContainer = $"dmp-data-{dmpSlot}";
    }
}
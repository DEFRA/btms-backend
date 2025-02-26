using Btms.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.BlobService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = configuration.GetSection(BlobServiceOptions.SectionName);

        services.BtmsAddOptions<BlobServiceOptions>(configuration, BlobServiceOptions.SectionName);

        services.PostConfigure<BlobServiceOptions>(x => x.CachePath =
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../../../", "btms-test-data",
                "Samples")));

        var blobOptions = config.Get<BlobServiceOptions>()!;

        if (blobOptions.CacheReadEnabled || blobOptions.CacheWriteEnabled)
        {
            services.AddKeyedSingleton<IBlobService, BlobService>("base");
            services.AddSingleton<IBlobService, CachingBlobService>();
        }
        else
        {
            services.AddSingleton<IBlobService, BlobService>();
        }

        services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();

        return services;
    }
}
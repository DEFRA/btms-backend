using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.BlobService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlobStorage(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<BlobServiceOptions>()
                .Bind(configuration.GetSection(BlobServiceOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
            services.AddSingleton<IBlobService, BlobService>();

            return services;
        }
    }
}
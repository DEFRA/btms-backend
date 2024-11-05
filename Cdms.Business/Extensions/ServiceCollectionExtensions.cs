using Cdms.Backend.Data.Extensions;
using Cdms.BlobService;
using Cdms.BlobService.Extensions;
using Cdms.Business.Commands;
using Cdms.SensitiveData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cdms.Business.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddOptions<SensitiveDataOptions>()
                .Bind(configuration.GetSection(SensitiveDataOptions.SectionName))
                .ValidateDataAnnotations();


            services.AddOptions<BlobServiceOptions>()
                .Bind(configuration.GetSection(BlobServiceOptions.SectionName))
                .ValidateDataAnnotations();

            services.AddOptions<BlobServiceOptions>()
                .Bind(configuration.GetSection(BlobServiceOptions.SectionName))
                .ValidateDataAnnotations();


            services.AddMongoDbContext(configuration);
            services.AddBlobStorage(configuration);
            services.AddSingleton<IBlobServiceClientFactory, BlobServiceClientFactory>();
            services.AddSingleton<ISensitiveDataSerializer, SensitiveDataSerializer>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SyncNotificationsCommand>());


            services.AddSingleton<SyncMetrics>();

            return services;
        }
    }
}
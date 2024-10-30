using Cdms.Backend.Data;
using Cdms.Backend.Data.Extensions;
using Cdms.BlobService;
using Cdms.BlobService.Extensions;
using Cdms.Business.Commands;
using Cdms.Business.Consumers;
using Cdms.Model.Extensions;
using Cdms.SensitiveData;
using Cdms.Types.Alvs;
using Cdms.Types.Gmr;
using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Interceptor;
using SlimMessageBus.Host.Memory;

namespace Cdms.Business.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(MetricsConsumerInterceptor<>));
            services.AddTransient(typeof(IMemoryConsumerErrorHandler<>), typeof(InMemoryConsumerErrorHandler<>));

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
            services.AddSingleton<IBlobService, BlobService.BlobService>();
            services.AddSingleton<ISensitiveDataSerializer, SensitiveDataSerializer>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SyncNotificationsCommand>());

            //Message Bus
            services.AddSlimMessageBus(mbb =>
            {
                mbb
                    .AddChildBus("InMemory", cbb =>
                    {
                        cbb.WithProviderMemory(cfg =>
                            {
                                cfg.EnableBlockingPublish = false;
                                cfg.EnableMessageHeaders = true;
                            })
                            .AddServicesFromAssemblyContaining<NotificationConsumer>(
                                consumerLifetime: ServiceLifetime.Scoped)
                            .Produce<ImportNotification>(x => x.DefaultTopic("NOTIFICATIONS"))
                            .Consume<ImportNotification>(x =>
                            {
                                x.Instances(10);
                                x.Topic("NOTIFICATIONS").WithConsumer<NotificationConsumer>();
                            })
                            .Produce<SearchGmrsForDeclarationIdsResponse>(x => x.DefaultTopic("GMR"))
                            .Consume<SearchGmrsForDeclarationIdsResponse>(x =>
                            {
                                x.Instances(10);
                                x.Topic("GMR").WithConsumer<GmrConsumer>();
                            })
                            .Produce<AlvsClearanceRequest>(x => x.DefaultTopic("ALVS"))
                            .Consume<AlvsClearanceRequest>(x =>
                            {
                                x.Instances(10);
                                x.Topic("ALVS").WithConsumer<AlvsClearanceRequestConsumer>();
                                // x.Topic("DECISIONS").WithConsumer<AlvsClearanceRequestConsumer>();
                            });
                    });

                //    .AddChildBus("ASB", cbb =>
                //{
                //     //Consume from ASB topics, but route to same consumers
                //});
            });

            services.AddSingleton<SyncMetrics>();

            return services;
        }
    }
}
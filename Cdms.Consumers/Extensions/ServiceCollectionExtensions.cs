using Cdms.Backend.Data.Extensions;
using Cdms.BlobService;
using Cdms.BlobService.Extensions;
using Cdms.Consumers.Interceptors;
using Cdms.Model.Alvs;
using Cdms.SensitiveData;
using Cdms.Types.Alvs;
using Cdms.Types.Gvms;
using Cdms.Types.Ipaffs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Interceptor;
using SlimMessageBus.Host.Memory;
using AlvsClearanceRequest = Cdms.Types.Alvs.AlvsClearanceRequest;

namespace Cdms.Consumers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsumers(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(MetricsConsumerInterceptor<>));
            services.AddTransient(typeof(IMemoryConsumerErrorHandler<>), typeof(InMemoryConsumerErrorHandler<>));

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
                                x.Instances(2);
                                x.Topic("NOTIFICATIONS").WithConsumer<NotificationConsumer>();
                            })
                            .Produce<SearchGmrsForDeclarationIdsResponse>(x => x.DefaultTopic("GMR"))
                            .Consume<SearchGmrsForDeclarationIdsResponse>(x =>
                            {
                                x.Instances(2);
                                x.Topic("GMR").WithConsumer<GmrConsumer>();
                            })
                            .Produce<AlvsClearanceRequest>(x => x.DefaultTopic(nameof(AlvsClearanceRequest)))
                            .Consume<AlvsClearanceRequest>(x =>
                            {
                                x.Instances(2);
                                x.Topic("ALVS").WithConsumer<AlvsClearanceRequestConsumer>();
                            })
                            .Consume<AlvsClearanceRequest>(x =>
                            {
                                x.Instances(2);
                                x.Topic("DECISIONS").WithConsumer<DecisionsConsumer>();
                            });
                    });
            });

            return services;
        }
    }
}
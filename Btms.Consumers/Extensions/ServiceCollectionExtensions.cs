using Btms.Common.Extensions;
using Btms.Consumers.Interceptors;
using Btms.Consumers.MemoryQueue;
using Btms.Metrics.Extensions;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AzureServiceBus;
using SlimMessageBus.Host.Interceptor;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.Serialization.SystemTextJson;
using AlvsClearanceRequest = Btms.Types.Alvs.AlvsClearanceRequest;
using Decision = Btms.Types.Alvs.Decision;

namespace Btms.Consumers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsumers(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.BtmsAddOptions<ConsumerOptions>(configuration, ConsumerOptions.SectionName);
            services.BtmsAddOptions<ServiceBusOptions>(configuration, ServiceBusOptions.SectionName);
            
            var consumerOpts = configuration
                .GetSection(ConsumerOptions.SectionName)
                .Get<ConsumerOptions>() ?? new ConsumerOptions();

            var serviceBusOptions = configuration
                .GetSection(ServiceBusOptions.SectionName)
                .Get<ServiceBusOptions>();

            services.AddBtmsMetrics();
            services.AddSingleton<IMemoryQueueStatsMonitor, MemoryQueueStatsMonitor>();
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(MetricsInterceptor<>));
            services.AddSingleton(typeof(IPublishInterceptor<>), typeof(MetricsInterceptor<>));
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(InMemoryQueueStatusInterceptor<>));
            services.AddSingleton(typeof(IPublishInterceptor<>), typeof(InMemoryQueueStatusInterceptor<>));
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(JobConsumerInterceptor<>));
            services.AddSingleton(typeof(IMemoryConsumerErrorHandler<>), typeof(InMemoryConsumerErrorHandler<>));

            //Message Bus
            services.AddSlimMessageBus(mbb =>
            {
                mbb.AddChildBus("ASB", cbb =>
                {
                    cbb.WithProviderServiceBus(cfg =>
                    {
                        cfg.ConnectionString = serviceBusOptions?.ConnectionString;
                    });
                    cbb.AddJsonSerializer();

                    cbb.Consume<object>(x => x
                        .Topic(serviceBusOptions?.AlvsSubscription.Topic)
                        .SubscriptionName(serviceBusOptions?.AlvsSubscription.Subscription)
                        .WithConsumer<AlvsAsbConsumer>()
                        .Instances(consumerOpts.AsbAlvsMessages));

                    cbb.Consume<ImportNotification>(x => x
                        .Topic(serviceBusOptions?.NotificationSubscription.Topic)
                        .SubscriptionName(serviceBusOptions?.NotificationSubscription.Subscription)
                        .WithConsumer<NotificationConsumer>()
                        .Instances(consumerOpts.AsbNotifications));
                });

                mbb
                    .AddChildBus("InMemory", cbb =>
                    {
                        cbb.WithProviderMemory(cfg =>
                            {
                                cfg.EnableBlockingPublish = consumerOpts.EnableBlockingPublish;
                                cfg.EnableMessageHeaders = true;
                            })
                            .AddServicesFromAssemblyContaining<NotificationConsumer>(
                                consumerLifetime: ServiceLifetime.Scoped)
                            .Produce<ImportNotification>(x => x.DefaultTopic("NOTIFICATIONS"))
                            .Consume<ImportNotification>(x =>
                            {
                                x.Instances(consumerOpts.InMemoryNotifications);
                                x.Topic("NOTIFICATIONS").WithConsumer<NotificationConsumer>();
                            })
                            .Produce<SearchGmrsForDeclarationIdsResponse>(x => x.DefaultTopic("GMR"))
                            .Consume<SearchGmrsForDeclarationIdsResponse>(x =>
                            {
                                x.Instances(consumerOpts.InMemoryGmrs);
                                x.Topic("GMR").WithConsumer<GmrConsumer>();
                            })
                            .Produce<AlvsClearanceRequest>(x => x.DefaultTopic(nameof(AlvsClearanceRequest)))
                            .Consume<AlvsClearanceRequest>(x =>
                            {
                                x.Instances(consumerOpts.InMemoryClearanceRequests);
                                x.Topic("CLEARANCEREQUESTS").WithConsumer<AlvsClearanceRequestConsumer>();
                            })
                            .Produce<Decision>(x => x.DefaultTopic(nameof(Decision)))
                            .Consume<Decision>(x =>
                            {
                                x.Instances(consumerOpts.InMemoryDecisions);
                                x.Topic("DECISIONS").WithConsumer<DecisionsConsumer>();
                            });
                    });

                    
            });

            return services;
        }
    }
}
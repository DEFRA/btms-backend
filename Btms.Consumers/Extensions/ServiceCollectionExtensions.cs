using System.Net;
using Azure.Messaging.ServiceBus;
using Btms.Common.Extensions;
using Btms.Consumers.AmazonQueues;
using Btms.Consumers.Interceptors;
using Btms.Consumers.MemoryQueue;
using Btms.Metrics.Extensions;
using Btms.Types.Alvs;
using Btms.Types.Gvms;
using Btms.Types.Ipaffs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AzureServiceBus;
using SlimMessageBus.Host.Interceptor;
using SlimMessageBus.Host.Memory;
using SlimMessageBus.Host.Serialization.SystemTextJson;
using AlvsClearanceRequest = Btms.Types.Alvs.AlvsClearanceRequest;
using Decision = Btms.Types.Alvs.Decision;
using ImportNotification = Btms.Types.Ipaffs.ImportNotification;

namespace Btms.Consumers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConsumers(this IServiceCollection services, IConfiguration configuration, ILogger logger)
        {
            services.BtmsAddOptions<ConsumerOptions>(configuration, ConsumerOptions.SectionName);
            services.BtmsAddOptions<ServiceBusOptions>(configuration, ServiceBusOptions.SectionName);

            var consumerOpts = configuration
                .GetSection(ConsumerOptions.SectionName)
                .Get<ConsumerOptions>() ?? new ConsumerOptions();

            var serviceBusOptions = configuration
                .GetSection(ServiceBusOptions.SectionName)
                .Get<ServiceBusOptions>() ?? throw new InvalidOperationException("Service bus options not found");

            services.AddBtmsMetrics();
            services.AddSingleton<IMemoryQueueStatsMonitor, MemoryQueueStatsMonitor>();
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(MetricsInterceptor<>));
            services.AddSingleton(typeof(IPublishInterceptor<>), typeof(MetricsInterceptor<>));
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(InMemoryQueueStatusInterceptor<>));
            services.AddSingleton(typeof(IPublishInterceptor<>), typeof(InMemoryQueueStatusInterceptor<>));
            services.AddSingleton(typeof(IConsumerInterceptor<>), typeof(JobConsumerInterceptor<>));
            services.AddSingleton(typeof(IMemoryConsumerErrorHandler<>), typeof(InMemoryConsumerErrorHandler<>));
            services.AddScoped<IClearanceRequestConsumer, ClearanceRequestConsumer>();
            services.AddScoped<AlvsClearanceRequestConsumer>();

            services.AddSlimMessageBus(mbb =>
            {
                if (consumerOpts.EnableAsbConsumers)
                {
                    mbb.AddChildBus("ASB_Notification", cbb =>
                    {
                        ConfigureServiceBusClient(cbb, serviceBusOptions.NotificationSubscription.ConnectionString);

                        cbb.Consume<ImportNotification>(x => x
                            .Topic(serviceBusOptions.NotificationSubscription.Topic)
                            .SubscriptionName(serviceBusOptions.NotificationSubscription.Subscription)
                            .WithConsumer<NotificationConsumer>()
                            .Instances(consumerOpts.AsbNotifications));
                    });

                    mbb.AddChildBus("ASB_Alvs", cbb =>
                    {
                        ConfigureServiceBusClient(cbb, serviceBusOptions.AlvsSubscription.ConnectionString);

                        cbb.Consume<object>(x => x
                            .Topic(serviceBusOptions.AlvsSubscription.Topic)
                            .SubscriptionName(serviceBusOptions.AlvsSubscription.Subscription)
                            .WithConsumer<AlvsAsbConsumer>()
                            .Instances(consumerOpts.AsbAlvsMessages));
                    });

                    mbb.AddChildBus("ASB_Gmr", cbb =>
                    {
                        ConfigureServiceBusClient(cbb, serviceBusOptions.GmrSubscription.ConnectionString);

                        cbb.Consume<Gmr>(x => x
                            .Topic(serviceBusOptions.GmrSubscription.Topic)
                            .SubscriptionName(serviceBusOptions.GmrSubscription.Subscription)
                            .WithConsumer<GmrConsumer>()
                            .Instances(consumerOpts.AsbGmrs));
                    });
                }

                mbb
                    .AddChildBus("InMemory", cbb =>
                    {
                        cbb.WithProviderMemory(cfg =>
                            {
                                cfg.EnableBlockingPublish = consumerOpts.EnableBlockingPublish;
                                cfg.EnableMessageHeaders = true;
                            })
                            .AddServicesFromAssemblyContaining<NotificationConsumer>(
                                consumerLifetime: ServiceLifetime.Scoped).PerMessageScopeEnabled()
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
                            .Produce<Gmr>(x => x.DefaultTopic("GMR"))
                            .Consume<Gmr>(x =>
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
                            })
                            .Produce<Finalisation>(x => x.DefaultTopic(nameof(Finalisation)))
                            .Consume<Finalisation>(x =>
                            {
                                x.Instances(consumerOpts.InMemoryFinalisations);
                                x.Topic("FINALISATIONS").WithConsumer<FinalisationsConsumer>();
                            });
                    });

                mbb.AddChildBus("AmazonQueues", cbb => cbb.AddAmazonConsumers(services, new AwsLocalOptions(configuration), logger));
            });

            return services;

            void ConfigureServiceBusClient(MessageBusBuilder cbb, string connectionString)
            {
                cbb.WithProviderServiceBus(cfg =>
                {
                    cfg.TopologyProvisioning = new ServiceBusTopologySettings { Enabled = false };
                    cfg.ClientFactory = (sp, settings) =>
                    {
                        var clientOptions = sp.GetRequiredService<IHostEnvironment>().IsDevelopment()
                            ? new ServiceBusClientOptions()
                            : new ServiceBusClientOptions
                            {
                                WebProxy = sp.GetRequiredService<IWebProxy>(),
                                TransportType = ServiceBusTransportType.AmqpWebSockets,
                            };

                        return new ServiceBusClient(settings.ConnectionString, clientOptions);
                    };
                    cfg.ConnectionString = connectionString;
                });
                cbb.AddJsonSerializer();
            }
        }
    }
}
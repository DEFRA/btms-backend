using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net;
using Btms.Consumers;

namespace Btms.Backend.Asb;

/// <summary>
/// Extension methods to configure
/// <see cref="AzureServiceBusHealthCheck{TOptions}"/>, <see cref="AzureServiceBusQueueHealthCheck"/>,
/// <see cref="AzureServiceBusSubscriptionHealthCheck"/>, <see cref="AzureServiceBusTopicHealthCheck"/>,
/// <see cref="AzureServiceBusQueueMessageCountThresholdHealthCheck"/>.
/// </summary>
public static class AzureServiceBusHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddBtmsAzureServiceBusSubscription(
        this IHealthChecksBuilder builder,
        TimeSpan? timeout = null)
    {
        builder.Add(new HealthCheckRegistration(
            "azuresubscription_alvs",
            sp =>
            {
                var sbOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var subscription = sbOptions.Value.AlvsSubscription;
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(subscription.Topic, subscription.Subscription)
                {
                    ConnectionString = sbOptions.Value.AlvsSubscription.ConnectionString,
                    UsePeekMode = true
                };
                return new AzureServiceBusSubscriptionHealthCheck(options, new BtmsServiceBusClientProvider(sp.GetRequiredService<IWebProxy>()));
            },
            null,
            null,
            timeout));
        
        builder.Add(new HealthCheckRegistration(
            "azuresubscription_notification",
            sp =>
            {
                var sbOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var subscription = sbOptions.Value.NotificationSubscription;
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(subscription.Topic, subscription.Subscription)
                {
                    ConnectionString = sbOptions.Value.NotificationSubscription.ConnectionString,
                    UsePeekMode = true
                };
                return new AzureServiceBusSubscriptionHealthCheck(options, new BtmsServiceBusClientProvider(sp.GetRequiredService<IWebProxy>()));
            },
            null,
            null,
            timeout));
        
        builder.Add(new HealthCheckRegistration(
            "azuresubscription_gmr",
            sp =>
            {
                var sbOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var subscription = sbOptions.Value.GmrSubscription;
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(subscription.Topic, subscription.Subscription)
                {
                    ConnectionString = sbOptions.Value.GmrSubscription.ConnectionString,
                    UsePeekMode = true
                };
                return new AzureServiceBusSubscriptionHealthCheck(options, new BtmsServiceBusClientProvider(sp.GetRequiredService<IWebProxy>()));
            },
            null,
            null,
            timeout));

        return builder;
    }
}
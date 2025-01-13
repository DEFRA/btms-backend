using Azure.Messaging.ServiceBus;
using Btms.Backend.Config;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Threading;
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
        TimeSpan? timeout = default)
    {
        builder.Add(new HealthCheckRegistration(
            "azuresubscription_alvs",
            sp =>
            {
                var sbOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var subscription = sbOptions.Value.AlvsSubscription;
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(subscription.Topic, subscription.Subscription)
                {
                    ConnectionString = sbOptions.Value.ConnectionString,
                    UsePeekMode = true
                };
                return new AzureServiceBusSubscriptionHealthCheck(options, new BtmsServiceBusClientProvider(sp.GetRequiredService<IWebProxy>()));
            },
            default,
            default,
            timeout));

        return builder.Add(new HealthCheckRegistration(
            "azuresubscription_notification",
            sp =>
            {
                var sbOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var subscription = sbOptions.Value.NotificationSubscription;
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(subscription.Topic, subscription.Subscription)
                {
                    ConnectionString = sbOptions.Value.ConnectionString,
                    UsePeekMode = true
                };
                return new AzureServiceBusSubscriptionHealthCheck(options, new BtmsServiceBusClientProvider(sp.GetRequiredService<IWebProxy>()));
            },
            default,
            default,
            timeout));
    }
}
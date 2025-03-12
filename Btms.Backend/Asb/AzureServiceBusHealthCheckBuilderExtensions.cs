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
            "azuresubscription_notification",
            sp => CreateHealthCheck(sp, sp.GetRequiredService<IOptions<ServiceBusOptions>>().Value.NotificationSubscription),
            null,
            null,
            timeout));

        return builder;
    }

    private static AzureServiceBusSubscriptionHealthCheck CreateHealthCheck(
        IServiceProvider sp,
        ServiceBusSubscriptionOptions subscription)
    {
        var options =
            new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(subscription.Topic, subscription.Subscription)
            {
                ConnectionString = subscription.ConnectionString,
                UsePeekMode = true
            };

        return new AzureServiceBusSubscriptionHealthCheck(options,
            new BtmsServiceBusClientProvider(sp.GetRequiredService<IWebProxy>()));
    }
}
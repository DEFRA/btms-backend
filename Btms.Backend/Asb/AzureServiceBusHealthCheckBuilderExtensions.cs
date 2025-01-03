using Btms.Backend.Config;
using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Net;

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

        return builder.Add(new HealthCheckRegistration(
            "azuresubscription",
            sp =>
            {
                var sbOptions = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
                var options = new AzureServiceBusSubscriptionHealthCheckHealthCheckOptions(sbOptions.Value.Topic, sbOptions.Value.Subscription)
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
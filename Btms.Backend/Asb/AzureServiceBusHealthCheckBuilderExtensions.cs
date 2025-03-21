using HealthChecks.AzureServiceBus;
using HealthChecks.AzureServiceBus.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using Btms.Consumers;
using Btms.Backend.Aws;
using Btms.Common.FeatureFlags;
using Microsoft.FeatureManagement;
using Btms.Common.Extensions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

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
        IConfiguration configuration,
        TimeSpan? timeout = null)
    {
        var options = builder.Services.BtmsAddOptions<ServiceBusOptions>(configuration, ServiceBusOptions.SectionName).Get();

        builder.Add(new HealthCheckRegistration(
            "azuresubscription_alvs",
            sp => BuildHealthCheck(sp, Features.HealthChecks.Asb.Alvs, options.AlvsSubscription),
            null,
            null,
            timeout));

        builder.Add(new HealthCheckRegistration(
            "azuresubscription_notification",
            sp => BuildHealthCheck(sp, Features.HealthChecks.Asb.Ipaffs, options.NotificationSubscription),
            null,
            null,
            timeout));

        builder.Add(new HealthCheckRegistration(
            "azuresubscription_gmr",
            sp => BuildHealthCheck(sp, Features.HealthChecks.Asb.Gmr, options.GmrSubscription),
            null,
            null,
            timeout));

        return builder;
    }

    private static IHealthCheck BuildHealthCheck(IServiceProvider sp, string feature, ServiceBusSubscriptionOptions subscription)
    {
        return new FeatureFlagHealthCheck(sp.GetRequiredService<IFeatureManager>(), feature, CreateHealthCheck(sp, subscription));
    }

    private static IHealthCheck CreateHealthCheck(
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
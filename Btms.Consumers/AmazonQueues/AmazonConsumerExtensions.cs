using Btms.Common;
using Btms.Types.Alvs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AmazonSQS;
using SlimMessageBus.Host.Serialization.SystemTextJson;

namespace Btms.Consumers.AmazonQueues;

internal static class AmazonConsumerExtensions
{
    public static void AddAmazonConsumers(this MessageBusBuilder mbb, IServiceCollection services, IConfiguration configuration)
    {
        mbb.WithProviderAmazonSQS(cfg =>
        {
            cfg.TopologyProvisioning.Enabled = false;
            SetConfigurationIfRequired(configuration, cfg);
        });

        mbb.AddJsonSerializer();

        mbb.AddConsumer<HmrcClearanceRequestConsumer, AlvsClearanceRequest>(services, "customs_clearance_request.fifo");
    }

    private static void SetConfigurationIfRequired(IConfiguration configuration, SqsMessageBusSettings cfg)
    {
        var logger = ApplicationLogging.LoggerFactory?.CreateLogger(nameof(AmazonConsumerExtensions));
        logger?.LogInformation("Configure AWS Consumer: ServiceURL={ServiceUrl}; AccessKeyId={AccessKeyId}; SecretAccessKey={SecretAccessKey}", configuration["AWS_ENDPOINT_URL"], configuration["AWS_ACCESS_KEY_ID"], configuration["AWS_SECRET_ACCESS_KEY"]);

        if (configuration["AWS_ENDPOINT_URL"] != null)
        {
            cfg.SqsClientConfig.ServiceURL = configuration["AWS_ENDPOINT_URL"];
            cfg.UseCredentials(configuration["AWS_ACCESS_KEY_ID"], configuration["AWS_SECRET_ACCESS_KEY"]);
        }
    }

    private static void AddConsumer<TConsumer, TMessage>(this MessageBusBuilder mbb, IServiceCollection services, string queueName) where TConsumer : MessageConsumer<TMessage> where TMessage : class
    {
        services.AddScoped<TConsumer>();
        mbb.Consume<MessageBody>(x => x
            .WithConsumer<TConsumer>()
            .Queue(queueName));
    }
}
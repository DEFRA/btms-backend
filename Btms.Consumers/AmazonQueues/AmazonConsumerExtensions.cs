using Btms.Types.Alvs;
using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AmazonSQS;
using SlimMessageBus.Host.Serialization.SystemTextJson;
using ILogger = Serilog.ILogger;

namespace Btms.Consumers.AmazonQueues;

internal static class AmazonConsumerExtensions
{
    public static void AddAmazonConsumers(this MessageBusBuilder mbb, IServiceCollection services, AwsLocalOptions awsLocalOptions, ILogger logger)
    {
        mbb.WithProviderAmazonSQS(cfg =>
        {
            cfg.TopologyProvisioning.Enabled = false;
            SetConfigurationIfRequired(awsLocalOptions, cfg, logger);
        });

        mbb.AddJsonSerializer();

        mbb.AddConsumer<HmrcClearanceRequestConsumer, AlvsClearanceRequest>(services, "customs_clearance_request.fifo");
    }

    private static void SetConfigurationIfRequired(AwsLocalOptions awsLocalOptions, SqsMessageBusSettings cfg, ILogger logger)
    {
        logger.Information("Configure AWS Consumer: ServiceURL={ServiceUrl}", awsLocalOptions.ServiceUrl);

        if (awsLocalOptions.ServiceUrl != null)
        {
            cfg.SqsClientConfig.ServiceURL = awsLocalOptions.ServiceUrl;
            cfg.UseCredentials(awsLocalOptions.AccessKeyId, awsLocalOptions.SecretAccessKey);
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
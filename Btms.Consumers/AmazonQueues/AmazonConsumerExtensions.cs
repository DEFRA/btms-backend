using Btms.Types.Alvs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AmazonSQS;
using SlimMessageBus.Host.Serialization.SystemTextJson;

namespace Btms.Consumers.AmazonQueues;

internal static class AmazonConsumerExtensions
{
    public static void AddAmazonConsumers(this MessageBusBuilder mbb, IServiceCollection services, AwsSqsOptions options, ILogger logger)
    {
        try
        {
            mbb.WithProviderAmazonSQS(cfg =>
            {
                cfg.TopologyProvisioning.Enabled = false;
                SetConfigurationIfRequired(options, cfg, logger);
            });

            mbb.AddJsonSerializer();

            mbb.AddConsumer<HmrcClearanceRequestConsumer, AlvsClearanceRequest>(services, options.ClearanceRequestQueueName, logger);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unable to configure AWS consumers");
        }
    }

    private static void SetConfigurationIfRequired(AwsSqsOptions options, SqsMessageBusSettings cfg, ILogger logger)
    {
        if (options.ServiceUrl != null)
        {
            logger.Information("Use AWS consumer Service URL {ServiceUrl}", options.ServiceUrl);

            cfg.SqsClientConfig.ServiceURL = options.ServiceUrl;
            cfg.UseCredentials(options.AccessKeyId, options.SecretAccessKey);
        }
    }

    private static void AddConsumer<TConsumer, TMessage>(this MessageBusBuilder mbb, IServiceCollection services, string queueName, ILogger logger) where TConsumer : MessageConsumer<TMessage> where TMessage : class
    {
        logger.Information("Added AWS consumer of type {ConsumerType} for messages of {MessageType} from {QueueName}", typeof(TConsumer).Name, typeof(TMessage).Name, queueName);

        services.AddScoped<TConsumer>();
        mbb.Consume<MessageBody>(x => x
            .WithConsumer<TConsumer>()
            .Queue(queueName));
    }
}
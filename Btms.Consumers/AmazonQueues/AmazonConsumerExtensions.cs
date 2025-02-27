using Btms.Types.Alvs;
using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AmazonSQS;
using SlimMessageBus.Host.Serialization.SystemTextJson;

namespace Btms.Consumers.AmazonQueues;

internal static class AmazonConsumerExtensions
{
    public static void AddAmazonConsumers(this MessageBusBuilder mbb, IServiceCollection services, AwsSqsOptions options)
    {
        mbb.WithProviderAmazonSQS(cfg =>
        {
            cfg.TopologyProvisioning.Enabled = false;
            SetConfigurationIfRequired(options, cfg);
        });

        mbb.AddJsonSerializer();
        
            //"customs_clearance_request.fifo"
        mbb.AddConsumer<HmrcClearanceRequestConsumer, AlvsClearanceRequest>(services, options.ClearanceRequestQueueName);
    }

    private static void SetConfigurationIfRequired(AwsSqsOptions options, SqsMessageBusSettings cfg)
    {
        cfg.SqsClientConfig.ServiceURL = options.ServiceUrl;
        cfg.UseCredentials(options.AccessKeyId, options.SecretAccessKey);
    }

    private static void AddConsumer<TConsumer, TMessage>(this MessageBusBuilder mbb, IServiceCollection services, string queueName) where TConsumer : MessageConsumer<TMessage> where TMessage : class
    {
        services.AddScoped<TConsumer>();
        mbb.Consume<MessageBody>(x => x
            .WithConsumer<TConsumer>()
            .Queue(queueName));
    }
}
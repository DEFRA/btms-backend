using Btms.Types.Alvs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var awsLocalOptions = configuration
            .GetSection("AwsOptions")
            .Get<AwsLocalOptions>();

        if (awsLocalOptions?.ServiceUrl != null)
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
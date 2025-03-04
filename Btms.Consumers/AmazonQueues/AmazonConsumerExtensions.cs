using Microsoft.Extensions.DependencyInjection;
using SlimMessageBus.Host;
using SlimMessageBus.Host.AmazonSQS;
using SlimMessageBus.Host.Serialization.SystemTextJson;

namespace Btms.Consumers.AmazonQueues;

internal static class AmazonConsumerExtensions
{
    public static void AddAmazonConsumers(this MessageBusBuilder mbb, IServiceCollection services,
        AwsSqsOptions options)
    {
        mbb.WithProviderAmazonSQS(cfg =>
        {
            cfg.TopologyProvisioning.Enabled = false;
            SetConfiguration(options, cfg);
        });

        mbb.AddJsonSerializer();
        mbb.Consume<MessageBody>(x => x
            .WithConsumer<SqsClearanceRequestConsumer>()
            .Queue(options.ClearanceRequestQueueName));
    }

    private static void SetConfiguration(AwsSqsOptions options, SqsMessageBusSettings cfg)
    {
        if (options.ServiceUrl != null)
        {
            cfg.SqsClientConfig.ServiceURL = options.ServiceUrl;
            cfg.UseCredentials(options.AccessKeyId, options.SecretAccessKey);
        }
        else
        {
            cfg.ClientProviderFactory = (provider => new CdpCredentialsSqsClientProvider(cfg.SqsClientConfig));
        }
    }
}
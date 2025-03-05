using Btms.Types.Alvs;
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
        mbb.Consume<AlvsClearanceRequest>(x => x
            .WithConsumer<ClearanceRequestConsumer>()
            .Queue(options.ClearanceRequestQueueName));

        mbb.Consume<Decision>(x => x
            .WithConsumer<DecisionsConsumer>()
            .Queue(options.DecisionQueueName));

        mbb.Consume<Finalisation>(x => x
            .WithConsumer<FinalisationsConsumer>()
            .Queue(options.FinalisationQueueName));
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
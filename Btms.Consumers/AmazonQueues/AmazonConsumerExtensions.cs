using Amazon;
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
        var awsLocalOptions = configuration
            .GetSection("AwsOptions")
            .Get<AwsLocalOptions>();

        mbb.WithProviderAmazonSQS(cfg =>
        {
            if (awsLocalOptions != null)
            {
                if (awsLocalOptions.ServiceUrl != null)
                    cfg.SqsClientConfig.ServiceURL = awsLocalOptions.ServiceUrl;
                else
                    cfg.UseRegion(RegionEndpoint.GetBySystemName(awsLocalOptions.Region));
                if (awsLocalOptions.AccessKeyId != null) cfg.UseCredentials(awsLocalOptions.AccessKeyId, awsLocalOptions.SecretAccessKey);
            }

            cfg.TopologyProvisioning.Enabled = false;
        });

        mbb.AddJsonSerializer();

        mbb.AddConsumer<HmrcClearanceRequestConsumer, AlvsClearanceRequest>(services, "customs_clearance_request.fifo");
    }

    private static void AddConsumer<TConsumer, TMessage>(this MessageBusBuilder mbb, IServiceCollection services, string queueName) where TConsumer : MessageConsumer<TMessage> where TMessage : class
    {
        services.AddScoped<TConsumer>();
        mbb.Consume<MessageBody>(x => x
            .WithConsumer<TConsumer>()
            .Queue(queueName));
    }
}
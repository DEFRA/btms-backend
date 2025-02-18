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
        var serviceUrl = configuration["AWS_SERVICE_URL"];
        var accessKey = configuration["AWS_ACCESS_KEY_ID"];
        var accessSecret = configuration["AWS_SECRET_ACCESS_KEY"];

        mbb.WithProviderAmazonSQS(cfg =>
        {
            cfg.SqsClientConfig.ServiceURL = serviceUrl;
            cfg.UseCredentials(accessKey, accessSecret);
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
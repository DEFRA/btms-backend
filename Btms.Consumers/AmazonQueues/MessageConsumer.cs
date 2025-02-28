using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal abstract class MessageConsumer<T>(ILogger logger) : MessageConsumer, IConsumer<MessageBody>, IConsumerWithContext where T : class
{
    public IConsumerContext Context { get; set; } = null!;

    public async Task OnHandle(MessageBody messageBody, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received message body with {TopicArn} of type {Type} with message: {Message}", messageBody.TopicArn, messageBody.Type, messageBody.Message);

        var message = JsonSerializer.Deserialize<T>(messageBody.Message, JsonSerializerOptions);
        if (message != null)
        {
            logger.LogInformation("Received none null message  of type {Type}", typeof(T).Name);

            await OnHandle(message, Context, cancellationToken);
        }
    }

    protected abstract Task OnHandle(T message, IConsumerContext context, CancellationToken cancellationToken);
}

internal abstract class MessageConsumer
{
    protected static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } };
}
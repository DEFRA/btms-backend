using System.Text.Json;
using System.Text.Json.Serialization;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal abstract class MessageConsumer<T> : IConsumer<MessageBody>, IConsumerWithContext where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } };

    public IConsumerContext Context { get; set; } = null!;

    public async Task OnHandle(MessageBody messageBody, CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize<T>(messageBody.Message, JsonSerializerOptions);
        if (message != null)
            await OnHandle(message, Context, cancellationToken);
    }

    protected abstract Task OnHandle(T message, IConsumerContext context, CancellationToken cancellationToken);
}
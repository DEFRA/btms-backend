// ReSharper disable InconsistentNaming

using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;

namespace Btms.Consumers.AmazonQueues;

public class MessageBody
{
    protected static readonly JsonSerializerOptions? JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() }
    };

    public required string Type { get; init; }
    public required string MessageId { get; init; }
    public required string TopicArn { get; init; }
    public required string Message { get; init; }
    public required string Timestamp { get; init; }
    public required string UnsubscribeURL { get; init; }
    public required string SequenceNumber { get; init; }

    public T? MessageAs<T>()
    {
        return JsonSerializer.Deserialize<T>(Message, JsonSerializerOptions);
    }
}
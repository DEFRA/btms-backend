// ReSharper disable InconsistentNaming

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Btms.Consumers.AmazonQueues;

public class MessageBody
{
    protected static readonly JsonSerializerOptions? JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Converters = { new JsonStringEnumConverter() } };

    public string? Type { get; set; }
    public string? MessageId { get; set; }
    public string? TopicArn { get; set; }
    public string? Message { get; set; }
    public string? Timestamp { get; set; }
    public string? UnsubscribeURL { get; set; }
    public string? SequenceNumber { get; set; }

    public T? MessageAs<T>()
    {
        return JsonSerializer.Deserialize<T>(Message!, JsonSerializerOptions);
    }
}
// ReSharper disable InconsistentNaming
namespace Btms.Consumers.AmazonQueues;

internal class MessageBody
{
    public required string Type { get; init; }
    public required string MessageId { get; init; }
    public required string TopicArn { get; init; }
    public required string Message { get; init; }
    public required string Timestamp { get; init; }
    public required string UnsubscribeURL { get; init; }
    public required string SequenceNumber { get; init; }
}
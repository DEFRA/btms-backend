using Btms.Backend.Data;
using Btms.Business.Builders;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal class SqsDecisionConsumer(IMongoDbContext dbContext, MovementBuilderFactory movementBuilderFactory, ILogger<DecisionsConsumer> logger)
    : IConsumer<MessageBody>, IConsumerWithContext
{
    public Task OnHandle(MessageBody message, CancellationToken cancellationToken)
    {
        var innerConsumer = new DecisionsConsumer(dbContext, movementBuilderFactory, logger);

        innerConsumer.Context = Context;

        var clearanceRequest = message.MessageAs<Decision>() ?? throw new InvalidOperationException();

        return innerConsumer.OnHandle(clearanceRequest, cancellationToken);
    }

    public IConsumerContext Context { get; set; } = null!;
}
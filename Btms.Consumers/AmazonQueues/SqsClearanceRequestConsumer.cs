using Btms.Backend.Data;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Decisions;
using Btms.Business.Services.Linking;
using Btms.Business.Services.Matching;
using Btms.Business.Services.Validating;
using Btms.Types.Alvs;
using Microsoft.Extensions.Logging;
using SlimMessageBus;

namespace Btms.Consumers.AmazonQueues;

internal class SqsClearanceRequestConsumer(
    IPreProcessor<AlvsClearanceRequest, Model.Movement> preProcessor,
    ILinkingService linkingService,
    IMatchingService matchingService,
    IDecisionService decisionService,
    IValidationService validationService,
    IMongoDbContext dbContext,
    ILogger<ClearanceRequestConsumer> logger)
    : IConsumer<MessageBody>, IConsumerWithContext
{
    public Task OnHandle(MessageBody message, CancellationToken cancellationToken)
    {
        var innerConsumer = new ClearanceRequestConsumer(preProcessor, linkingService, matchingService, decisionService,
            validationService, dbContext, logger);

        innerConsumer.Context = Context;

        var clearanceRequest = message.MessageAs<AlvsClearanceRequest>() ?? throw new InvalidOperationException();

        return innerConsumer.OnHandle(clearanceRequest, cancellationToken);
    }

    public IConsumerContext Context { get; set; } = null!;
}
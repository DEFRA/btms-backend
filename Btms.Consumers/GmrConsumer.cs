using Btms.Backend.Data;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Business.Services.Linking;
using Btms.Consumers.Extensions;
using Btms.Model.Ipaffs;
using Btms.Types.Gvms;
using SlimMessageBus;
using SearchGmrsForDeclarationIdsResponse = Btms.Types.Gvms.SearchGmrsForDeclarationIdsResponse;

namespace Btms.Consumers;

internal class GmrConsumer(
    IMongoDbContext mongoDbContext,
    IPreProcessor<Gmr, Model.Gvms.Gmr> preProcessor,
    ILinker<ImportNotification, Model.Gvms.Gmr> linker)
    : IConsumer<SearchGmrsForDeclarationIdsResponse>, IConsumer<Gmr>, IConsumerWithContext
{
    public async Task OnHandle(SearchGmrsForDeclarationIdsResponse message, CancellationToken cancellationToken)
    {
        foreach (var gmr in message.Gmrs!)
            await HandleGmr(gmr, cancellationToken);

        await mongoDbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
    }

    public async Task OnHandle(Gmr message, CancellationToken cancellationToken)
    {
        await HandleGmr(message, cancellationToken);
        await mongoDbContext.SaveChangesAsync(cancellation: Context.CancellationToken);
    }

    private async Task HandleGmr(Gmr gmr, CancellationToken cancellationToken)
    {
        var result = await preProcessor.Process(
            new PreProcessingContext<Gmr>(gmr, Context.GetMessageId()),
            cancellationToken);

        if (result.IsCreatedOrChanged())
        {
            var linkResult = await linker.Link(result.Record, cancellationToken);

            // We need to mark the entity as updated even if the conceptual resource has not changed
            // so that consumers of BTMS can query notifications where related data has changed but
            // the resource itself hasn't
            await mongoDbContext.Notifications.Update(linkResult.From.ToList(), cancellationToken);
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
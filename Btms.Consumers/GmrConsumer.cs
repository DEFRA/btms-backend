using Btms.Backend.Data;
using Btms.Business.Pipelines.PreProcessing;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using Btms.Types.Gvms;
using SlimMessageBus;
using SearchGmrsForDeclarationIdsResponse = Btms.Types.Gvms.SearchGmrsForDeclarationIdsResponse;

namespace Btms.Consumers;

internal class GmrConsumer(IMongoDbContext mongoDbContext, IPreProcessor<Gmr, Model.Gvms.Gmr> preProcessor)
    : IConsumer<SearchGmrsForDeclarationIdsResponse>, IConsumer<Gmr>, IConsumerWithContext
{
    public async Task OnHandle(SearchGmrsForDeclarationIdsResponse message, CancellationToken cancellationToken)
    {
        foreach (var gmr in message.Gmrs!)
            await HandleGmr(gmr, cancellationToken);

        await mongoDbContext.SaveChangesAsync(Context.CancellationToken);
    }

    public async Task OnHandle(Gmr message, CancellationToken cancellationToken)
    {
        await HandleGmr(message, cancellationToken);
        await mongoDbContext.SaveChangesAsync(Context.CancellationToken);
    }

    private async Task HandleGmr(Gmr gmr, CancellationToken cancellationToken)
    {
        var result = await preProcessor.Process(
            new PreProcessingContext<Gmr>(gmr, Context.GetMessageId()),
            cancellationToken);
            
        if (result.IsCreatedOrChanged())
            await LinkImportNotifications(result.Record, cancellationToken);
    }

    private async Task LinkImportNotifications(Model.Gvms.Gmr mappedGmr, CancellationToken cancellationToken)
    {
        var mrns = mappedGmr.Declarations?.Customs?
            .Select(x => x.Id)
            .NotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase) ?? [];
        
        foreach (var mrn in mrns)
        {
            var notification = mongoDbContext.Notifications.FirstOrDefault(x =>
                    x.ExternalReferences != null &&
                    x.ExternalReferences.Any(y =>
                        y.System == ExternalReferenceSystemEnum.Ncts &&
#pragma warning disable CA1862 
                        // MongoDB driver does not support string.Equals()
                        y.Reference != null && y.Reference.ToLowerInvariant() == mrn.ToLowerInvariant()));
#pragma warning restore CA1862

            if (notification is null ||
                notification.Relationships.Gmrs.Data.Any(x =>
                    string.Equals(x.Id, mappedGmr.Id, StringComparison.OrdinalIgnoreCase)))
                continue;
            
            // GMR relationship does not already exist so add it
            notification.Relationships.Gmrs.Data.Add(new RelationshipDataItem
            {
                Type = LinksBuilder.Gmr.ResourceName,
                Id = mappedGmr.Id,
                Links = new ResourceLink
                {
                    Self = LinksBuilder.BuildSelfLink(LinksBuilder.Gmr.ResourceName, mappedGmr.Id!)
                }
            });
            
            await mongoDbContext.Notifications.Update(notification, cancellationToken);
        }
    }

    public IConsumerContext Context { get; set; } = null!;
}
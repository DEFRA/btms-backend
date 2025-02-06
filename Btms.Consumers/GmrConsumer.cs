using Btms.Backend.Data;
using Btms.Common.Extensions;
using Btms.Consumers.Extensions;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;
using Btms.Types.Gvms;
using Btms.Types.Gvms.Mapping;
using SlimMessageBus;
using SearchGmrsForDeclarationIdsResponse = Btms.Types.Gvms.SearchGmrsForDeclarationIdsResponse;

namespace Btms.Consumers;

internal class GmrConsumer(IMongoDbContext mongoDbContext)
    : IConsumer<SearchGmrsForDeclarationIdsResponse>, IConsumer<Gmr>, IConsumerWithContext
{
    public async Task OnHandle(SearchGmrsForDeclarationIdsResponse message, CancellationToken cancellationToken)
    {
        foreach (var gmr in message.Gmrs!)
        {
            var mappedGmr = await SaveOrUpdateGmr(gmr, Context.GetMessageId(), Context.CancellationToken);

            await LinkImportNotifications(mappedGmr, cancellationToken);
        }

        await mongoDbContext.SaveChangesAsync(Context.CancellationToken);
    }

    public async Task OnHandle(Gmr message, CancellationToken cancellationToken)
    {
        var mappedGmr = await SaveOrUpdateGmr(message, Context.GetMessageId(), Context.CancellationToken);

        await LinkImportNotifications(mappedGmr, cancellationToken);

        await mongoDbContext.SaveChangesAsync(Context.CancellationToken);
    }

    private async Task<Model.Gvms.Gmr> SaveOrUpdateGmr(Gmr message, string auditId, CancellationToken cancellationToken)
    {
        var mappedGmr = GrmWithTransformMapper.MapWithTransform(message);
        var existingGmr = await mongoDbContext.Gmrs.Find(mappedGmr.Id!);

        if (existingGmr is null)
        {
            var auditEntry = AuditEntry.CreateCreatedEntry(
                mappedGmr,
                auditId,
                1,
                message.UpdatedSource,
                CreatedBySystem.Gvms);

            mappedGmr.AuditEntries.Add(auditEntry);

            await mongoDbContext.Gmrs.Insert(mappedGmr, cancellationToken);
        }
        else
        {
            if (message.UpdatedSource > existingGmr.UpdatedSource)
            {
                mappedGmr.AuditEntries = existingGmr.AuditEntries;

                var auditEntry = AuditEntry.CreateUpdated(
                    existingGmr,
                    mappedGmr,
                    auditId,
                    mappedGmr.AuditEntries.Count + 1,
                    message.UpdatedSource,
                    CreatedBySystem.Gvms);

                mappedGmr.AuditEntries.Add(auditEntry);

                await mongoDbContext.Gmrs.Update(mappedGmr, existingGmr._Etag, cancellationToken);
            }
        }

        return mappedGmr;
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

            var notifications = mongoDbContext.Notifications.ToList();

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
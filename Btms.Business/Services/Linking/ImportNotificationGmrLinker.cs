using Btms.Backend.Data;
using Btms.Common.Extensions;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;

namespace Btms.Business.Services.Linking;

public class ImportNotificationGmrLinker(IMongoDbContext mongoDbContext) : ILinker<ImportNotification, Gmr>
{
    public async Task Link(Gmr model, CancellationToken cancellationToken)
    {
        var mrns = model.Declarations?.Customs?
            .Select(x => x.Id)
            .NotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase) ?? [];
        
        foreach (var mrn in mrns)
        {
            var notification = await FindNotification(mrn, cancellationToken);
            if (notification is null)
                continue;

            await AddGmrRelationshipIfNotPresentAndUpdate(model, notification, cancellationToken);
            await AddNotificationRelationshipIfNotPresentAndUpdate(model, notification, cancellationToken);
        }
    }

    private async Task<ImportNotification?> FindNotification(string mrn, CancellationToken cancellationToken)
    {
        var notification = await mongoDbContext.Notifications.Find(x =>
            x.ExternalReferences != null &&
            x.ExternalReferences.Any(y =>
                y.System == ExternalReferenceSystemEnum.Ncts &&
#pragma warning disable CA1862 
                // MongoDB driver does not support string.Equals()
                y.Reference != null && y.Reference.ToLowerInvariant() == mrn.ToLowerInvariant()), cancellationToken);
#pragma warning restore CA1862
        
        return notification;
    }

    private async Task AddGmrRelationshipIfNotPresentAndUpdate(Gmr model, ImportNotification notification, CancellationToken cancellationToken)
    {
        if (notification.Relationships.Gmrs.Data.Any(x => Match(x.Id, model.Id)))
            return;
            
        notification.Relationships.Gmrs.Data.Add(new RelationshipDataItem
        {
            Type = LinksBuilder.Gmr.ResourceName,
            Id = model.Id,
            Links = new ResourceLink
            {
                Self = LinksBuilder.BuildSelfLink(LinksBuilder.Gmr.ResourceName, model.Id!)
            }
        });
        
        await mongoDbContext.Notifications.Update(notification, cancellationToken);
    }

    private async Task AddNotificationRelationshipIfNotPresentAndUpdate(Gmr model, ImportNotification notification, CancellationToken cancellationToken)
    {
        if (model.Relationships.ImportNotifications.Data.Any(x => Match(x.Id, notification.Id)))
            return;
            
        model.Relationships.ImportNotifications.Data.Add(new RelationshipDataItem
        {
            Type = LinksBuilder.Notification.ResourceName,
            Id = notification.Id,
            Links = new ResourceLink
            {
                Self = LinksBuilder.BuildSelfLink(LinksBuilder.Notification.ResourceName, notification.Id!)
            }
        });
        
        await mongoDbContext.Gmrs.Update(model, cancellationToken);
    }

    private static bool Match(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
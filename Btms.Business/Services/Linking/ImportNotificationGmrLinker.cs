using Btms.Backend.Data;
using Btms.Backend.Data.Extensions;
using Btms.Common.Extensions;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;

namespace Btms.Business.Services.Linking;

public class ImportNotificationGmrLinker(IMongoDbContext mongoDbContext) : ILinker<ImportNotification, Gmr>
{
    public async Task<LinkerResult<ImportNotification, Gmr>> Link(Gmr model, CancellationToken cancellationToken)
    {
        var transits = model.Declarations?.Transits?.Select(x => x.Id).NotNull() ?? [];
        var customs = model.Declarations?.Customs?.Select(x => x.Id).NotNull() ?? [];
        var mrns = transits
            .Concat(customs)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var notifications = await FindNotifications(mrns, cancellationToken);

        foreach (var notification in notifications)
        {
            await AddGmrRelationshipIfNotPresentAndUpdate(model, notification, cancellationToken);
            await AddNotificationRelationshipIfNotPresentAndUpdate(model, notification, cancellationToken);
        }

        return new LinkerResult<ImportNotification, Gmr>(notifications, model);
    }
    
    private async Task<List<ImportNotification>> FindNotifications(string[] mrns, CancellationToken cancellationToken)
    {
        if (mrns.Length == 0) return [];
        
        return await mongoDbContext.Notifications.Where(x =>
            x.ExternalReferences != null &&
            x.ExternalReferences.Any(y => mrns.Any(mrn =>
                y.System == ExternalReferenceSystemEnum.Ncts &&
                y.Reference != null &&
#pragma warning disable CA1862
                // MongoDB driver does not support string.Equals()
                y.Reference.ToLowerInvariant() == mrn.ToLowerInvariant()))).ToListAsync(cancellationToken);
#pragma warning restore CA1862
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
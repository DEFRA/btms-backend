using Btms.Backend.Data;
using Btms.Backend.Data.Extensions;
using Btms.Common.Extensions;
using Btms.Model.Gvms;
using Btms.Model.Ipaffs;
using Btms.Model.Relationships;

namespace Btms.Business.Services.Linking;

public class ImportNotificationGmrLinker(IMongoDbContext mongoDbContext)
    : ILinker<ImportNotification, Gmr>, ILinker<Gmr, ImportNotification>
{
    public async Task<LinkerResult<ImportNotification, Gmr>> Link(Gmr model, CancellationToken cancellationToken)
    {
        var transits = model.Declarations?.Transits?.Select(x => x.Id).NotNull() ?? [];
        var customs = model.Declarations?.Customs?.Select(x => x.Id).NotNull() ?? [];
        var mrns = transits
            .Concat(customs)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        var notifications = await FindNotifications(mrns.ToArray(), cancellationToken);

        foreach (var notification in notifications)
        {
            await AddGmrRelationshipIfNotPresentAndUpdate(notification, model, cancellationToken);
            await AddNotificationRelationshipIfNotPresentAndUpdate(model, notification, cancellationToken);
        }

        return new LinkerResult<ImportNotification, Gmr>(notifications, model);
    }

    public async Task<LinkerResult<Gmr, ImportNotification>> Link(
        ImportNotification model,
        CancellationToken cancellationToken)
    {
        var mrns = model.ExternalReferences?
            .Where(x => x.System == ExternalReferenceSystemEnum.Ncts && !string.IsNullOrWhiteSpace(x.Reference))
            .Select(x => x.Reference)
            .NotNull()
            .Distinct() ?? [];
        var gmrs = await FindGmrs(mrns.ToArray(), cancellationToken);

        foreach (var gmr in gmrs)
        {
            await AddGmrRelationshipIfNotPresentAndUpdate(model, gmr, cancellationToken);
            await AddNotificationRelationshipIfNotPresentAndUpdate(gmr, model, cancellationToken);
        }

        return new LinkerResult<Gmr, ImportNotification>(gmrs, model);
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
                    y.Reference.ToLowerInvariant() == mrn.ToLowerInvariant())))
#pragma warning restore CA1862
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Gmr>> FindGmrs(string[] mrns, CancellationToken cancellationToken)
    {
        if (mrns.Length == 0) return [];

        return await mongoDbContext.Gmrs.Where(x =>
                x.Declarations != null &&
                ((
                    x.Declarations.Transits != null &&
                    x.Declarations.Transits.Any(y => mrns.Any(mrn =>
                        y.Id != null &&
#pragma warning disable CA1862
                        // MongoDB driver does not support string.Equals()
                        y.Id.ToLowerInvariant() == mrn.ToLowerInvariant()))
                ) || (
                    x.Declarations.Customs != null &&
                    x.Declarations.Customs.Any(y => mrns.Any(mrn =>
                        y.Id != null &&
#pragma warning disable CA1862
                        // MongoDB driver does not support string.Equals()
                        y.Id.ToLowerInvariant() == mrn.ToLowerInvariant()))
                )))
#pragma warning restore CA1862
            .ToListAsync(cancellationToken);
    }

    private async Task AddGmrRelationshipIfNotPresentAndUpdate(
        ImportNotification notification,
        Gmr gmr,
        CancellationToken cancellationToken)
    {
        if (notification.Relationships.Gmrs.Data.Any(x => Match(x.Id, gmr.Id)))
            return;

        notification.Relationships.Gmrs.Data.Add(new RelationshipDataItem
        {
            Type = LinksBuilder.Gmr.ResourceName,
            Id = gmr.Id,
            Links = new ResourceLink { Self = LinksBuilder.BuildSelfLink(LinksBuilder.Gmr.ResourceName, gmr.Id!) }
        });

        await mongoDbContext.Notifications.Update(notification, cancellationToken);
    }

    private async Task AddNotificationRelationshipIfNotPresentAndUpdate(
        Gmr gmr,
        ImportNotification notification,
        CancellationToken cancellationToken)
    {
        if (gmr.Relationships.ImportNotifications.Data.Any(x => Match(x.Id, notification.Id)))
            return;

        gmr.Relationships.ImportNotifications.Data.Add(new RelationshipDataItem
        {
            Type = LinksBuilder.Notification.ResourceName,
            Id = notification.Id,
            Links = new ResourceLink
            {
                Self = LinksBuilder.BuildSelfLink(LinksBuilder.Notification.ResourceName, notification.Id!)
            }
        });

        await mongoDbContext.Gmrs.Update(gmr, cancellationToken);
    }

    private static bool Match(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

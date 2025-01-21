using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public sealed class RelationshipDataItem
{
    [Attr] public string? Type { get; set; }

    [Attr] public string? Id { get; set; }

    [Attr] public ResourceLink? Links { get; set; }

    [Attr] public int? SourceItem { get; set; }

    [Attr] public int? DestinationItem { get; set; }

    public int? MatchingLevel { get; set; }

    public Dictionary<string, object?> ToDictionary()
    {
        var meta = new Dictionary<string, object?>();

        if (SourceItem.HasValue)
        {
            meta.Add("sourceItem", SourceItem);
        }

        if (DestinationItem.HasValue)
        {
            meta.Add("destinationItem", DestinationItem);
        }

        if (MatchingLevel.HasValue)
        {
            meta.Add("matchingLevel", MatchingLevel);
        }

        if (!string.IsNullOrEmpty(Links?.Self))
        {
            meta.Add("self", Links.Self);
        }

        return meta;
    }

    public static RelationshipDataItem CreateFromNotification(ImportNotification notification, Movement movement, string matchReference)
    {
        return new RelationshipDataItem
        {
            Type = "notifications",
            Id = notification.Id!,
            SourceItem = movement.Items
                .Find(x => x.Documents!.ToList().Exists(d => d.DocumentReference!.Contains(matchReference)))
                ?.ItemNumber,
            DestinationItem = notification.Commodities.FirstOrDefault()?.ComplementId,
            Links = new ResourceLink { Self = LinksBuilder.Notification.BuildSelfNotificationLink(notification.Id!) },
            MatchingLevel = 1
        };
    }

    public static RelationshipDataItem CreateFromMovement(ImportNotification notification, Movement movement, string matchReference)
    {
        return new RelationshipDataItem
        {
            Type = "movements",
            Id = movement.Id!,
            SourceItem = notification.Commodities.FirstOrDefault()?.ComplementId,
            DestinationItem = movement.Items
                .Find(x => x.Documents!.ToList().Exists(d => d.DocumentReference!.Contains(matchReference)))
                ?.ItemNumber,
            Links = new ResourceLink { Self = LinksBuilder.Movement.BuildRelatedMovementLink(movement.Id!) },
            MatchingLevel = 1
        };
    }
}
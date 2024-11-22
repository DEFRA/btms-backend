using Cdms.Model.Extensions;
using Cdms.Model.Ipaffs;
using JsonApiDotNetCore.Resources.Annotations;

namespace Cdms.Model.Relationships;

public sealed class RelationshipDataItem
{
    [Attr] public bool? Matched { get; set; } = default!;

    [Attr] public string Type { get; set; } = default!;

    [Attr] public string Id { get; set; } = default!;

    [Attr] public ResourceLink Links { get; set; } = default!;

    [Attr] public int? SourceItem { get; set; } = default!;

    [Attr] public int? DestinationItem { get; set; } = default!;

    public int? MatchingLevel { get; set; }

    public Dictionary<string, object?> ToDictionary()
    {
        var meta = new Dictionary<string, object?>();
        if (Matched.HasValue)
        {
            meta.Add("matched", Matched);
        }

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

    public static RelationshipDataItem CreateFromNotification(ImportNotification notification, Movement movement,
        string matchReference, bool matched = true, string reason = null!)
    {
        Dictionary<string, string> additionalInfo = new Dictionary<string, string>() { { "matchingLevel", "1" } };

        if (!string.IsNullOrEmpty(reason))
        {
            additionalInfo.Add("reason", reason);
        }

        return new RelationshipDataItem()
        {
            Matched = matched,
            Type = "notifications",
            Id = notification.Id!,
            SourceItem = movement.Items
                .Find(x => x.Documents!.ToList().Exists(d => d.DocumentReference!.Contains(matchReference)))
                ?.ItemNumber,
            DestinationItem = notification.Commodities?.FirstOrDefault()?.ComplementId,
            Links = new ResourceLink() { Self = LinksBuilder.Notification.BuildSelfNotificationLink(notification.Id!) },
            MatchingLevel = 1
        };
    }

    public static RelationshipDataItem CreateFromMovement(ImportNotification notification, Movement movement,
        string matchReference, bool matched = true, string reason = null!)
    {
        Dictionary<string, string> additionalInfo = new Dictionary<string, string>() { { "matchingLevel", "1" } };

        if (!string.IsNullOrEmpty(reason))
        {
            additionalInfo.Add("reason", reason);
        }

        return new RelationshipDataItem()
        {
            Matched = matched,
            Type = "movements",
            Id = movement.Id!,
            SourceItem = notification?.Commodities?.FirstOrDefault()?.ComplementId,
            DestinationItem = movement.Items
                .Find(x => x.Documents!.ToList().Exists(d => d.DocumentReference!.Contains(matchReference)))
                ?.ItemNumber,
            Links = new ResourceLink() { Self = LinksBuilder.Movement.BuildRelatedMovementLink(movement.Id!) },
            MatchingLevel = 1
        };
    }
}
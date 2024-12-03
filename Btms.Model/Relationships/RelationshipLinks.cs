using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using JsonApiDotNetCore.Resources.Annotations;

namespace Btms.Model.Relationships;

public sealed class RelationshipLinks
{
    [Attr] public string Self { get; set; } = default!;

    [Attr] public string Related { get; set; } = default!;

    public static RelationshipLinks CreateForMovement(Movement movement)
    {
        return new RelationshipLinks()
        {
            Self = LinksBuilder.Movement.BuildSelfMovementLink(movement.Id!),
            Related = LinksBuilder.Movement.BuildRelatedMovementLink(movement.Id!)
        };
    }

    public static RelationshipLinks CreateForNotification(ImportNotification notification)
    {
        return new RelationshipLinks()
        {
            Self = LinksBuilder.Notification.BuildSelfNotificationLink(notification.Id!),
            Related = LinksBuilder.Notification.BuildRelatedMovementLink(notification.Id!)
        };
    }
}
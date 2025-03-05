using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public class LinkResult(LinkOutcome state)
{
    public LinkOutcome Outcome { get; set; } = state;
    public List<ImportNotification> Notifications { get; set; } = new();
    public List<Movement> Movements { get; set; } = new();

    public bool IsAllNotificationsDeleted()
    {
        return Notifications.All(x => x.Status == ImportNotificationStatusEnum.Deleted);
    }
}
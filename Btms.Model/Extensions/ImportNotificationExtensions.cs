using Btms.Model.Cds;
using Btms.Model.Ipaffs;

namespace Btms.Model.Extensions;

public static class ImportNotificationExtensions
{
    public static DecisionImportNotifications AsDecisionImportNotification(this ImportNotification notification)
    {
        return new DecisionImportNotifications
        {
            Id = notification.Id!,
            Version = notification.Version,
            Created = notification.Created,
            Updated = notification.Updated,
            UpdatedEntity = notification.UpdatedEntity,
            CreatedSource = notification.CreatedSource!.Value,
            UpdatedSource = notification.UpdatedSource!.Value,
            AutoClearedOn = notification.PartTwo != null ? notification.PartTwo!.AutoClearedOn : null,
            Status = notification.Status, //!.Value,
            Type = notification.ImportNotificationType!.Value
        };
    }
}
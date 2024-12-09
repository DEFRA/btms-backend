using Btms.Model;
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services;

public abstract record LinkContext
{
    public static MovementLinkContext ForMovement(Movement receivedMovement, ChangeSet? changeSet = null)
    {
        return new MovementLinkContext(receivedMovement, changeSet);
    }

    public static ImportNotificationLinkContext ForImportNotification(ImportNotification receivedImportNotification, ChangeSet? changeSet = null)
    {
        return new ImportNotificationLinkContext(receivedImportNotification, changeSet);
    }

    public abstract string GetIdentifiers();
}
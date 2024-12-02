using Cdms.Model;
using Cdms.Model.Ipaffs;

namespace Cdms.Business.Services;

public abstract record LinkContext
{
    public static MovementLinkContext ForMovement(Movement receivedMovement, Movement? existingMovement = null)
    {
        return new MovementLinkContext(receivedMovement, existingMovement);
    }

    public static ImportNotificationLinkContext ForImportNotification(ImportNotification receivedImportNotification, ImportNotification? existingImportNotification = null)
    {
        return new ImportNotificationLinkContext(receivedImportNotification, existingImportNotification);
    }

    public abstract string GetIdentifiers();
}
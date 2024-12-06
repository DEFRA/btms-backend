using Btms.Model.Ipaffs;

namespace Btms.Business.Services;

public record ImportNotificationLinkContext(ImportNotification PersistedImportNotification, ImportNotification? ExistingImportNotification) : LinkContext
{
    public override string GetIdentifiers()
    {
        return PersistedImportNotification._MatchReference;
    }
}
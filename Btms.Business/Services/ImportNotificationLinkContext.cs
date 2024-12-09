using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services;

public record ImportNotificationLinkContext(ImportNotification PersistedImportNotification, ChangeSet? ChangeSet) : LinkContext
{
    public override string GetIdentifiers()
    {
        return PersistedImportNotification._MatchReference;
    }
}
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public record ImportNotificationLinkContext(ImportNotification PersistedImportNotification, ChangeSet? ChangeSet) : LinkContext
{
    public override string GetIdentifiers()
    {
        return PersistedImportNotification._MatchReference;
    }
}
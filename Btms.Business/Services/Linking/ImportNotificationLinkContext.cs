using Btms.Model;
using Btms.Model.ChangeLog;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public record ImportNotificationLinkContext(ImportNotification PersistedImportNotification, ChangeSet? ChangeSet, List<Movement>? Movements = null) : LinkContext
{
    public override string GetIdentifiers()
    {
        return PersistedImportNotification._MatchReference;
    }
}
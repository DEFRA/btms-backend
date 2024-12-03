using Btms.Model.Ipaffs;

namespace Btms.Business.Services;

public record ImportNotificationLinkContext(ImportNotification ReceivedImportNotification, ImportNotification? ExistingImportNotification) : LinkContext
{
    public override string GetIdentifiers()
    {
        return ReceivedImportNotification._MatchReference;
    }
}
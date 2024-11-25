using Cdms.Model.Ipaffs;

namespace Cdms.Business.Services;

public record ImportNotificationLinkContext(ImportNotification ReceivedImportNotification, ImportNotification? ExistingImportNotification) : LinkContext;
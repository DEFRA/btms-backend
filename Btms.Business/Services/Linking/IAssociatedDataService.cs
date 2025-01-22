using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public interface IAssociatedDataService
{
    Task Update(List<ImportNotification> notifications, string auditEntryId, CancellationToken cancellationToken);
}
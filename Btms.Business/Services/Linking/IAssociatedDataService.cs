using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public interface IAssociatedDataService
{
    /// <summary>
    /// Signal on all specified notifications that a related data entity has changed.
    /// </summary>
    /// <param name="notifications"></param>
    /// <param name="auditId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RelatedDataEntityChanged(List<ImportNotification> notifications, string auditId, CancellationToken cancellationToken);
}
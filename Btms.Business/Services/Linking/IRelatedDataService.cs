using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public interface IRelatedDataService
{
    /// <summary>
    /// Signal on all specified notifications that a related data entity has changed.
    /// </summary>
    /// <param name="notifications"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RelatedDataEntityChanged(List<ImportNotification> notifications, CancellationToken cancellationToken);
}
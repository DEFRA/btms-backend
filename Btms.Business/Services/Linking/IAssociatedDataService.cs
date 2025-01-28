using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Linking;

public interface IAssociatedDataService
{
    Task UpdateRelationships(List<ImportNotification> notifications, Movement movement, CancellationToken cancellationToken);
}
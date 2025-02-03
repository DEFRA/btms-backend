using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Matching;

public class MatchingContext(
    List<ImportNotification> notifications,
    List<Movement> movements)
{
    public List<ImportNotification> Notifications { get; } = notifications;
    public List<Movement> Movements { get; } = movements;
}
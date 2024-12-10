using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions;

public class DecisionContext
{
    public List<ImportNotification> Notifications { get; set; } = new();
    public List<Movement> Movements { get; set; } = new();
}
using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services;

public class LinkResult(LinkOutcome state)
{
    public LinkOutcome Outcome { get; set; } = state;
    public List<ImportNotification> Notifications { get; set; } = new();
    public List<Movement> Movements { get; set; } = new();
}
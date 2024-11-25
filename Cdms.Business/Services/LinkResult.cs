using Cdms.Model;
using Cdms.Model.Ipaffs;

namespace Cdms.Business.Services;

public class LinkResult(LinkState state)
{
    public LinkState State { get; set; } = state;
    public List<ImportNotification> Notifications { get; set; } = new();
    public List<Movement> Movements { get; set; } = new();
}
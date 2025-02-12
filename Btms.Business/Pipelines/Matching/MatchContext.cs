using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Pipelines.Matching;

public class MatchContext
{
    public List<ImportNotification> Notifications { get; set; } = new();
    public List<Movement> Movements { get; set; } = new();
    public string MatchReference { get; set; } = string.Empty;
    public bool ContinueMatching { get; set; } = true;

    // Debugging
    public string Record { get; set; } = string.Empty;
}
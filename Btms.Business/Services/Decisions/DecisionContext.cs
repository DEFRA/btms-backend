using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Ipaffs;

namespace Btms.Business.Services.Decisions;

public class DecisionContext(
    List<ImportNotification> notifications,
    List<Movement> movements,
    MatchingResult matchingResult,
    string messageId)
{
    public List<ImportNotification> Notifications { get; } = notifications;
    public List<Movement> Movements { get; } = movements;
    public MatchingResult MatchingResult { get; } = matchingResult;

    public string MessageId { get; } = messageId;
}
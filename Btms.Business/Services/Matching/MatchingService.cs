using System.Diagnostics;
using Btms.Business.Extensions;
using Btms.Model;

namespace Btms.Business.Services.Matching;

public class MatchingService : IMatchingService
{
    public Task<MatchingResult> Process(MatchingContext matchingContext, CancellationToken cancellationToken)
    {
        var matchingResult = new MatchingResult();
        foreach (var movement in matchingContext.Movements)
        {
            foreach (var item in movement.Items)
            {
                if (item.Documents == null) continue;

                foreach (var documentGroup in item
                             .Documents
                             .Where(MovementExtensions.IsChed)
                             .GroupBy(d => d.DocumentReference))
                {
                    Debug.Assert(documentGroup.Key != null);
                    Debug.Assert(movement.Id != null, "movement.Id != null");
                    Debug.Assert(item.ItemNumber != null, "item.ItemNumber != null");

                    var notification = matchingContext.Notifications.Find(x =>
                        x._MatchReference == MatchIdentifier.FromCds(documentGroup.Key).Identifier);

                    if (notification is null)
                    {
                        matchingResult.AddDocumentNoMatch(movement.Id, item.ItemNumber.Value, documentGroup.Key);
                    }
                    else
                    {
                        Debug.Assert(notification?.Id != null, "notification.Id != null");
                        matchingResult.AddMatch(notification.Id, movement.Id, item.ItemNumber.Value, documentGroup.Key);
                    }
                }
            }
        }

        return Task.FromResult(matchingResult);
    }
}
using Cdms.Backend.Data;

namespace Cdms.Analytics;

public class MatchingAggregationService(IMongoDbContext context) : IMatchingAggregationService
{
    public Task<ByDateResult[]> GetImportNotificationsByMatchStatus()
    {
        var s = context
            .Notifications
            .GroupBy(n => new
            {
                Matched = n.Relationships.Movements.Matched.HasValue && n.Relationships.Movements.Matched.Value,
                n.CreatedSource!.Value.Date,
                n.ImportNotificationType
            })
            .AsEnumerable()
            .Select(r => new ByDateResult()
            {
                Date = DateOnly.FromDateTime(r.Key.Date),
                BucketVariables = new Dictionary<string, string>()
                {
                    {
                        "Matched", r.Key.Matched.ToString()
                    },
                    {
                        "ChedType", r.Key.ImportNotificationType.ToString()!
                    }
                },
                Value = r.Count()
            })
            .ToArray();

        return Task.FromResult(s);
    }
}
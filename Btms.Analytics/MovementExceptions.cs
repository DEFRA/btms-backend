using Btms.Analytics.Extensions;
using Btms.Backend.Data;
using Microsoft.Extensions.Logging;

namespace Btms.Analytics;

public class MovementExceptions(IMongoDbContext context, ILogger logger)
{
    //Returns a summary of the exceptions or a list
    // Means we can share the same anonymous / query code without needing to create loads
    // of classes
    public (SingleSeriesDataset summary, List<ExceptionResult>) GetAllExceptions(DateTime from, DateTime to, bool summary, string[]? chedTypes = null, string? country = null)
    {
        var exceptionsSummary = new SingleSeriesDataset();
        var exceptionsResult = new List<ExceptionResult>();
        
        var simplifiedMovementView = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .Where(m => country == null || m.DispatchCountryCode == country )
            .Select(m => new
            {
                Id = m.Id,
                UpdatedSource = m.UpdatedSource,
                Updated = m.Updated,
                MaxDecisionNumber = m.Decisions.Max(d => d.Header!.DecisionNumber) ?? 0,
                MaxEntryVersion = m.ClearanceRequests.Max(c => c.Header!.EntryVersionNumber) ?? 0,
                LinkedCheds = m.Relationships.Notifications.Data.Count,
                ItemCount = m.Items.Count,
                HasMatchDecisions = m.AlvsDecisions.Any(d => d.Context.AlvsAnyMatch),
                HasNotificationRelationships = m.Relationships.Notifications.Data.Count > 0
            })
            .Select(m => new
            {
                Id = m.Id,
                UpdatedSource = m.UpdatedSource,
                Updated = m.Updated,
                MaxDecisionNumber = m.MaxDecisionNumber,
                MaxEntryVersion = m.MaxEntryVersion,
                LinkedCheds = m.LinkedCheds,
                ItemCount = m.ItemCount,
                HasMatchDecisions = m.HasMatchDecisions,
                HasNotificationRelationships = m.HasNotificationRelationships,
                Total = m.MaxDecisionNumber + m.MaxEntryVersion + m.LinkedCheds + m.ItemCount,
                // TODO - can we include CHED versions here too?
                TotalDocumentVersions = m.MaxDecisionNumber + m.MaxEntryVersion + m.LinkedCheds
            });

        var moreComplexMovementsQuery = simplifiedMovementView
            .Where(r => r.TotalDocumentVersions > 5);

        if (summary)
        {
            exceptionsSummary.Values.Add("complexMovement", moreComplexMovementsQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(moreComplexMovementsQuery
                .OrderBy(a => -a.Total)
                .Take(10)
                .Execute(logger)
                .Select(r =>
                    new ExceptionResult()
                    {
                        Resource = "Movement",
                        Id = r.Id!,
                        UpdatedSource = r.UpdatedSource!.Value,
                        Updated = r.Updated,
                        ItemCount = r.ItemCount,
                        MaxEntryVersion = r.MaxEntryVersion,
                        MaxDecisionNumber = r.MaxDecisionNumber,
                        LinkedCheds = r.LinkedCheds,
                        Reason = "High Number Of Messages"
                    })
            );
        }
        
        var movementsWhereAlvsLinksButNotBtmsQuery = simplifiedMovementView
            .Where(r => r.HasMatchDecisions && !r.HasNotificationRelationships);
        
        if (summary)
        {
            exceptionsSummary.Values.Add("alvsLinksButNotBtms", movementsWhereAlvsLinksButNotBtmsQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(movementsWhereAlvsLinksButNotBtmsQuery
                .OrderBy(a => -a.Total)
                .Take(10)
                .Execute(logger)
                .Select(r =>
                    new ExceptionResult()
                    {
                        Resource = "Movement",
                        Id = r.Id!,
                        UpdatedSource = r.UpdatedSource!.Value,
                        Updated = r.Updated,
                        ItemCount = r.ItemCount,
                        MaxEntryVersion = r.MaxEntryVersion,
                        MaxDecisionNumber = r.MaxDecisionNumber,
                        LinkedCheds = r.LinkedCheds,
                        Reason = "Alvs has match decisions but we don't have links"
                    })
            );
        }

        return (exceptionsSummary, exceptionsResult);
    }
}
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
                // TODO - we should think about pre-calculating this stuff and storing it on the movement...

                Id = m.Id,
                UpdatedSource = m.UpdatedSource,
                Updated = m.Updated,
                MaxDecisionNumber = m.Decisions.Max(d => d.Header!.DecisionNumber) ?? 0,
                MaxEntryVersion = m.ClearanceRequests.Max(c => c.Header!.EntryVersionNumber) ?? 0,
                LinkedCheds = m.Relationships.Notifications.Data.Count,
                ItemCount = m.Items.Count,
                HasMatchDecisions = m.AlvsDecisions.Any(d => d.Context.AlvsAnyMatch),
                DecisionMatched = !m.AlvsDecisions
                    .OrderBy(d => d.Context.AlvsDecisionNumber)
                    .Reverse()
                    .First()
                    .Context.DecisionMatched,
                HasNotificationRelationships = m.Relationships.Notifications.Data.Count > 0,
                ContiguousAlvsClearanceRequestVersionsFrom1 = 
                    m.ClearanceRequests.Select(c => c.Header!.EntryVersionNumber)
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
                TotalDocumentVersions = m.MaxDecisionNumber + m.MaxEntryVersion + m.LinkedCheds,
                DecisionMatched = m.DecisionMatched,
                ContiguousAlvsClearanceRequestVersionsFrom1 = m.ContiguousAlvsClearanceRequestVersionsFrom1.Count() == m.MaxEntryVersion
            });

        var moreComplexMovementsQuery = simplifiedMovementView
            .Where(r => r.TotalDocumentVersions > 5);

        if (summary)
        {
            exceptionsSummary.Values.Add("Complex Movement", moreComplexMovementsQuery.Count());
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
            exceptionsSummary.Values.Add("Alvs has match decisions but no Btms links", movementsWhereAlvsLinksButNotBtmsQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(movementsWhereAlvsLinksButNotBtmsQuery
                .OrderBy(a => a.Total)
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
                        Reason = "Alvs has match decisions but no Btms links"
                    })
            );
        }
        
        var movementsWhereWeHaveAndContigousVersionsButDecisionsAreDifferentQuery = simplifiedMovementView
            .Where(r =>
                r.ContiguousAlvsClearanceRequestVersionsFrom1 && r.DecisionMatched
                && r.HasNotificationRelationships);
        
        if (summary)
        {
            exceptionsSummary.Values.Add("BTMS Links But Decision Wrong", movementsWhereWeHaveAndContigousVersionsButDecisionsAreDifferentQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(movementsWhereWeHaveAndContigousVersionsButDecisionsAreDifferentQuery
                .OrderBy(a => a.Total)
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
                        Reason = "BTMS Links But Decision Wrong"
                    })
            );
        }

        return (exceptionsSummary, exceptionsResult);
    }
}
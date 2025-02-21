using System.Diagnostics.CodeAnalysis;
using System.Text;
using Btms.Analytics.Extensions;
using Btms.Backend.Data;
using Btms.Common.Enum;
using Btms.Common.Extensions;
using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Microsoft.Extensions.Logging;

namespace Btms.Analytics;

public class MovementExceptions(IMongoDbContext context, ILogger logger)
{
    private const string Movement = "Movement";

    private static string FormatUnmatched(DecisionStatusEnum decisionStatus, MovementStatusEnum? status, MovementSegmentEnum? segment)
    {
        var matched = new StringBuilder($"{decisionStatus}");
        if (status.HasValue()) matched.Append($" {status}");
        if (segment.HasValue()) matched.Append($" : {segment}");

        return matched.ToString();
    }

    [SuppressMessage("SonarLint", "S3776",
        Justification =
            "Means we can share the same anonymous / query code without needing to create loads of classes for the intermediate state")]
    public (SingleSeriesDataset summary, List<ExceptionResult>) GetAllExceptions(DateTime from, DateTime to, bool finalisedOnly, bool summary, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var exceptionsSummary = new SingleSeriesDataset();
        var exceptionsResult = new List<ExceptionResult>();

        var simplifiedMovementView = context
            .Movements
            .WhereFilteredByCreatedDateAndParams(from, to, finalisedOnly, chedTypes, country)
            .Select(m => new
            {
                // NB - we should think about pre-calculating this stuff and storing it on the movement...

                m.Id,
                m.UpdatedSource,
                m.UpdatedEntity,
                m.Updated,
                MaxDecisionNumber = m.Decisions.Max(d => d.Header!.DecisionNumber) ?? 0,
                MaxEntryVersion = m.ClearanceRequests.Max(c => c.Header!.EntryVersionNumber) ?? 0,
                LinkedCheds = m.Relationships.Notifications.Data.Count,
                ItemCount = m.Items.Count,
                m.BtmsStatus.ChedTypes,
                m.BtmsStatus.LinkStatus,
                m.BtmsStatus.Status,
                m.BtmsStatus.Segment,
                m.BtmsStatus.BusinessDecisionStatus,
                m.AlvsDecisionStatus.Context.DecisionComparison,
                HasNotificationRelationships = m.Relationships.Notifications.Data.Count > 0,
                ContiguousAlvsClearanceRequestVersionsFrom1 =
                    m.ClearanceRequests.Select(c => c.Header!.EntryVersionNumber)
            })
            .Select(m => new
            {
                m.Id,
                m.UpdatedSource,
                m.UpdatedEntity,
                m.Updated,
                m.MaxDecisionNumber,
                m.MaxEntryVersion,
                m.LinkedCheds,
                m.ItemCount,
                m.ChedTypes,
                m.LinkStatus,
                m.Status,
                m.Segment,
                DecisionStatus = (m.DecisionComparison == null) ? DecisionStatusEnum.None : m.DecisionComparison!.DecisionStatus,
                m.BusinessDecisionStatus,
                DecisionMatched = (m.DecisionComparison != null) && m.DecisionComparison!.DecisionMatched,
                m.HasNotificationRelationships,
                Total = m.MaxDecisionNumber + m.MaxEntryVersion + m.LinkedCheds + m.ItemCount,
                TotalDocumentVersions = m.MaxDecisionNumber + m.MaxEntryVersion + m.LinkedCheds,
                ContiguousAlvsClearanceRequestVersionsFrom1 = m.ContiguousAlvsClearanceRequestVersionsFrom1.Count() == m.MaxEntryVersion
            });

        var unMatchedGroupedByDecisionStatus = simplifiedMovementView
            .Where(r => !r.DecisionMatched)
            .GroupBy(r => new { r.DecisionStatus, r.Status, r.Segment })
            .Execute(logger);


        if (summary)
        {
            foreach (var g in unMatchedGroupedByDecisionStatus)
            {
                exceptionsSummary.Values.Add(FormatUnmatched(g.Key.DecisionStatus, g.Key.Status, g.Key.Segment), g.Count());
            }
        }
        else
        {
            foreach (var g in unMatchedGroupedByDecisionStatus)
            {
                exceptionsResult.AddRange(g
                    .OrderBy(a => -a.Total)
                    .Take(10)
                    .Select(r =>
                        new ExceptionResult()
                        {
                            Resource = Movement,
                            Id = r.Id!,
                            UpdatedSource = r.UpdatedSource!.Value,
                            UpdatedEntity = r.UpdatedEntity,
                            Updated = r.Updated,
                            ItemCount = r.ItemCount,
                            ChedTypes = r.ChedTypes!,
                            MaxEntryVersion = r.MaxEntryVersion,
                            MaxDecisionNumber = r.MaxDecisionNumber,
                            LinkedCheds = r.LinkedCheds,
                            Reason = FormatUnmatched(g.Key.DecisionStatus, g.Key.Status, g.Key.Segment)
                        })
                );
            }
        }


        var unMatchedGroupedByBusinessDecisionStatus = simplifiedMovementView
            .Where(r => !r.DecisionMatched)
            .GroupBy(r => r.BusinessDecisionStatus)
            .Execute(logger);

        if (summary)
        {
            foreach (var g in unMatchedGroupedByBusinessDecisionStatus)
            {
                exceptionsSummary.Values.Add(g.Key.GetValue(), g.Count());
            }
        }
        else
        {
            foreach (var g in unMatchedGroupedByBusinessDecisionStatus)
            {
                exceptionsResult.AddRange(g
                    .OrderBy(a => -a.Total)
                    .Take(10)
                    .Select(r =>
                        new ExceptionResult()
                        {
                            Resource = Movement,
                            Id = r.Id!,
                            UpdatedSource = r.UpdatedSource!.Value,
                            UpdatedEntity = r.UpdatedEntity,
                            Updated = r.Updated,
                            ItemCount = r.ItemCount,
                            ChedTypes = r.ChedTypes!,
                            MaxEntryVersion = r.MaxEntryVersion,
                            MaxDecisionNumber = r.MaxDecisionNumber,
                            LinkedCheds = r.LinkedCheds,
                            Reason = g.Key.GetValue()
                        })
                );
            }
        }

        var moreComplexMovementsQuery = simplifiedMovementView
            .Where(m => m.LinkStatus != LinkStatusEnum.AllLinked
                && m.TotalDocumentVersions > 5);

        if (summary)
        {
            exceptionsSummary.Values.Add("Complex Movement", moreComplexMovementsQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(moreComplexMovementsQuery
                .OrderBy(a => -a.Total)
                .Take(3)
                .Execute(logger)
                .Select(r =>
                    new ExceptionResult
                    {
                        Resource = Movement,
                        Id = r.Id!,
                        UpdatedSource = r.UpdatedSource!.Value,
                        UpdatedEntity = r.UpdatedEntity,
                        Updated = r.Updated,
                        ItemCount = r.ItemCount,
                        ChedTypes = r.ChedTypes!,
                        MaxEntryVersion = r.MaxEntryVersion,
                        MaxDecisionNumber = r.MaxDecisionNumber,
                        LinkedCheds = r.LinkedCheds,
                        Reason = "High Number Of Messages"
                    })
            );
        }

        var movementsWhereAlvsLinksButNotBtmsQuery = simplifiedMovementView
            .Where(r => r.LinkStatus != LinkStatusEnum.AllLinked);

        if (summary)
        {
            exceptionsSummary.Values.Add("Alvs has match decisions but no Btms links", movementsWhereAlvsLinksButNotBtmsQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(movementsWhereAlvsLinksButNotBtmsQuery
                .OrderBy(a => a.Total)
                .Take(3)
                .Execute(logger)
                .Select(r =>
                    new ExceptionResult
                    {
                        Resource = Movement,
                        Id = r.Id!,
                        UpdatedSource = r.UpdatedSource!.Value,
                        UpdatedEntity = r.UpdatedEntity,
                        Updated = r.Updated,
                        ItemCount = r.ItemCount,
                        ChedTypes = r.ChedTypes!,
                        MaxEntryVersion = r.MaxEntryVersion,
                        MaxDecisionNumber = r.MaxDecisionNumber,
                        LinkedCheds = r.LinkedCheds,
                        Reason = "Alvs has match decisions but no Btms links"
                    })
            );
        }

        var movementsWhereWeHaveAndContigousVersionsButDecisionsAreDifferentQuery = simplifiedMovementView
            .Where(r =>
                r.LinkStatus != LinkStatusEnum.AllLinked
                && r.ContiguousAlvsClearanceRequestVersionsFrom1 && r.DecisionMatched
                && r.HasNotificationRelationships);

        if (summary)
        {
            exceptionsSummary.Values.Add("BTMS Links But Decision Wrong", movementsWhereWeHaveAndContigousVersionsButDecisionsAreDifferentQuery.Count());
        }
        else
        {
            exceptionsResult.AddRange(movementsWhereWeHaveAndContigousVersionsButDecisionsAreDifferentQuery
                .OrderBy(a => a.Total)
                .Take(3)
                .Execute(logger)
                .Select(r =>
                    new ExceptionResult
                    {
                        Resource = Movement,
                        Id = r.Id!,
                        UpdatedSource = r.UpdatedSource!.Value,
                        UpdatedEntity = r.UpdatedEntity,
                        Updated = r.Updated,
                        ItemCount = r.ItemCount,
                        ChedTypes = r.ChedTypes!,
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
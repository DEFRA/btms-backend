using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Btms.Analytics.Extensions;
using Btms.Backend.Data;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
using Btms.Model;
using Btms.Model.Cds;
using Btms.Model.Auditing;
using Btms.Model.Ipaffs;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Btms.Analytics;

public class MovementsAggregationService(IMongoDbContext context, ILogger<MovementsAggregationService> logger) : IMovementsAggregationService
{
    /// <summary>
    /// Aggregates movements by createdSource and returns counts by date period. Could be refactored to use a generic/interface in time
    /// </summary>
    /// <param name="from">Time period to search from (inclusive)</param>
    /// <param name="to">Time period to search to (exclusive)</param>
    /// <param name="aggregateBy">Aggregate by day/hour</param>
    /// <returns></returns>
    public Task<MultiSeriesDatetimeDataset> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<Movement, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) => AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean());

        return Aggregate(dateRange, CreateDatasetName, matchFilter, "$createdSource", aggregateBy);
    }

    public Task<SingleSeriesDataset> ByStatus(DateTime from, DateTime to)
    {
        var data = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => m.Relationships.Notifications.Data.Count > 0)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(g => AnalyticsHelpers.GetLinkedName(g.Key), g => g.Count);
            
        return Task.FromResult(new SingleSeriesDataset
        {
            Values = AnalyticsHelpers.GetMovementSegments().ToDictionary(title => title, title => data.GetValueOrDefault(title, 0))
        });
    }

    public Task<MultiSeriesDataset> ByItemCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => new { Linked = m.Relationships.Notifications.Data.Count > 0, ItemCount = m.Items.Count })
            .Select(g => new { g.Key.Linked, g.Key.ItemCount, Count = g.Count() });
            
        var mongoResult = mongoQuery
            .Execute(logger)
            .ToList();
            
        var dictionary = mongoResult
            .ToDictionary(g => new { Title = AnalyticsHelpers.GetLinkedName(g.Linked), g.ItemCount }, g => g.Count);
            
        var maxCount = mongoResult.Count > 0 ?
            mongoResult.Max(r => r.Count) : 0;

        return Task.FromResult(new MultiSeriesDataset()
        {
            Series = AnalyticsHelpers.GetMovementSegments()
                .Select(title => new Series()
                    {
                        Name = title,
                        Dimension = "Item Count",
                        Results = Enumerable.Range(0, maxCount + 1)
                            .Select(i => new ByNumericDimensionResult
                            {
                                Dimension = i,
                                Value = dictionary.GetValueOrDefault(new { Title=title, ItemCount = i }, 0)
                            }).ToList<IDimensionResult>()
                    }
                )
                .ToList()    
        });
    }
    
    public Task<MultiSeriesDataset> ByUniqueDocumentReferenceCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => new
            {
                Linked = m.Relationships.Notifications.Data.Count > 0,
                DocumentReferenceCount = m.Items
                    .SelectMany(i => i.Documents == null ? new string[] {} : i.Documents.Select(d => d.DocumentReference))
                    .Distinct()
                    .Count()
            })
            .Select(g => new { g.Key.Linked, g.Key.DocumentReferenceCount, MovementCount = g.Count() });
            
        var mongoResult = mongoQuery.Execute(logger).ToList();
        
        var dictionary = mongoResult
            .ToDictionary(
                g => new { Title = AnalyticsHelpers.GetLinkedName(g.Linked), g.DocumentReferenceCount },
                g => g.MovementCount);

        var maxReferences = mongoResult.Count > 0 ?
            mongoResult.Max(r => r.DocumentReferenceCount) : 0;

        return Task.FromResult(new MultiSeriesDataset()
        {
            Series = AnalyticsHelpers.GetMovementSegments()
                .Select(title => new Series()
                {
                    Name = title,
                    Dimension = "Document Reference Count",
                    Results = Enumerable.Range(0, maxReferences + 1)
                        .Select(i => new ByNumericDimensionResult
                        {
                            Dimension = i,
                            Value = dictionary.GetValueOrDefault(new { Title = title, DocumentReferenceCount = i },
                                0)
                        }).ToList<IDimensionResult>()
                })
                .ToList()
        });
    }

    public Task<SingleSeriesDataset> UniqueDocumentReferenceByMovementCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(m => m.CreatedSource >= from && m.CreatedSource < to)
            .SelectMany(m => m.Items.Select(i => new { Item = i, MovementId = m.Id }))
            .SelectMany(i => i.Item.Documents!.Select(d =>
                new { i.MovementId, d.DocumentReference }))
            .Distinct()
            .GroupBy(d => d.DocumentReference)
            .Select(d => new { DocumentReference = d.Key, MovementCount = d.Count() })
            .GroupBy(d => d.MovementCount)
            .Select(d => new { MovementCount = d.Key, DocumentReferenceCount = d.Count() });
            
            var mongoResult = mongoQuery
                .Execute(logger)
                .ToDictionary(
                    r =>r.MovementCount.ToString(),
                    r=> r.DocumentReferenceCount);

            var result = new SingleSeriesDataset { Values = mongoResult };
            
            return Task.FromResult(result);
    }

    public async Task<EntityDataset<AuditHistory>?> GetHistory(string movementId)
    {
        var movement = await context
            .Movements
            .Find(movementId);

        if (!movement.HasValue())
        {
            return null;
        }
        
        var notificationIds = movement!.Relationships.Notifications.Data.Select(n => n.Id);

        var notificationEntries = context.Notifications
            .Where(n => notificationIds.Contains(n.Id))
            .SelectMany(n => n.AuditEntries
                .Select(a => 
                    new AuditHistory(a, $"ImportNotification", n.Id!)
                )
            );
        
        var entries = movement!.AuditEntries
            .Select(a => new AuditHistory(a, "Movement", movementId))
            .Concat(notificationEntries);

        entries = entries.OrderBy(a => a.AuditEntry.CreatedSource);
        
        return new EntityDataset<AuditHistory>(entries);
    }

    public Task<SingleSeriesDataset> ByMaxVersion(DateTime from, DateTime to, string[]? chedTypes = null, string? country = null)
    {
        var data = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .Where(m => country == null || m.DispatchCountryCode == country )
            .GroupBy(n => new { MaxVersion =
                n.ClearanceRequests.Max(a => a.Header!.EntryVersionNumber )
            })
            .Select(g => new { MaxVersion = g.Key.MaxVersion ?? 0, Count = g.Count() })
            .ExecuteAsSortedDictionary(logger, g => g.MaxVersion, g => g.Count);

        return Task.FromResult(new SingleSeriesDataset
        {
            Values = data.ToDictionary(d => d.Key.ToString(), d => d.Value)
        });
    }
    
    public Task<SingleSeriesDataset> ByMaxDecisionNumber(DateTime from, DateTime to, string[]? chedTypes = null, string? country = null)
    {
        var data = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .Where(m => country == null || m.DispatchCountryCode == country )
            .GroupBy(n => new { MaxVersion =
                n.Decisions.Max(a => a.Header!.DecisionNumber )
            })
            .Select(g => new { MaxVersion = g.Key.MaxVersion ?? 0, Count = g.Count() })
            .ExecuteAsSortedDictionary(logger, g => g.MaxVersion, g => g.Count);

        return Task.FromResult(new SingleSeriesDataset
        {
            Values = data.ToDictionary(d => d.Key.ToString(), d => d.Value)
        });
    }

    public Task<List<ExceptionResult>> GetExceptions(DateTime from, DateTime to, string[]? chedTypes = null, string? country = null)
    {
        var movementExceptions = new MovementExceptions(context, logger);
        var (_, result) = movementExceptions.GetAllExceptions(from, to, false, chedTypes, country);
            
        return Task.FromResult(result);
    }
    
    public Task<SingleSeriesDataset> ExceptionSummary(DateTime from, DateTime to, string[]? chedTypes = null, string? country = null)
    {
        var movementExceptions = new MovementExceptions(context, logger);
        var (summary, _) = movementExceptions.GetAllExceptions(from, to, true, chedTypes, country);
            
        return Task.FromResult(summary);
    }

    // TODO : remove
    // public Task<MultiSeriesDataset> ByCheck(DateTime from, DateTime to, string[]? chedTypes = null, string? country = null)
    // {
    //     return Task.FromResult(new MultiSeriesDataset() );
    // }

    private Task<MultiSeriesDatetimeDataset> Aggregate(DateTime[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<Movement, bool>> filter, string dateField, AggregationPeriod aggregateBy)
    {
        var truncateBy = aggregateBy == AggregationPeriod.Hour ? "hour" : "day";
        
        ProjectionDefinition<Movement> projection = "{linked:{ $ne: [0, { $size: '$relationships.notifications.data'}]}, dateToUse: { $dateTrunc: { date: '" + dateField + "', unit: '" + truncateBy + "'}}}";
        
        // First aggregate the dataset by whether its matched and the date it arrives. Count the number in each bucket.
        ProjectionDefinition<BsonDocument> group = "{_id: { linked: '$linked', dateToUse: '$dateToUse' }, count: { $count: { } }}";
        
        // Then further group by whether its matched and get the count to give us the structure we need in our chart
        ProjectionDefinition<BsonDocument> datasetGroup = "{_id: { linked: '$_id.linked'}, dates: { $push: { dateToUse: '$_id.dateToUse', count: '$count' }}}";

        var mongoResult = context
            .Movements
            .GetAggregatedRecordsDictionary(filter, projection, group, datasetGroup, createDatasetName);

        var output = AnalyticsHelpers.GetMovementSegments()
            .Select(title => mongoResult.AsDataset(dateRange, title))
            .AsOrderedArray(m => m.Name);
        
        logger.LogDebug("Aggregated Data {Result}", output.ToList().ToJsonString());
        
        return Task.FromResult(new MultiSeriesDatetimeDataset() { Series = output.ToList() });
    }

    /// <summary>
    /// Finds the most recent decision from Alvs and BTMS
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public Task<SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>> ByDecision(DateTime from,
        DateTime to, string[]? chedTypes = null, string? country = null)
    {
        var mongoQuery = context
            .Movements
            .Where(m => m.CreatedSource >= from && m.CreatedSource < to)
            .SelectMany(m => m.AlvsDecisions.Select(
                d => new {Decision = d, Movement = m } ))
            .SelectMany(d => d.Decision.Checks.Select(c => new { d.Decision, d.Movement, Check = c}))
            .GroupBy(d => new
            {
                d.Decision.Context.DecisionStatus,
                d.Check.CheckCode,
                d.Check.AlvsDecisionCode,
                d.Check.BtmsDecisionCode
            })
            .Select(g => new
            {
                g.Key, Count = g.Count()
            })
            .Execute(logger);
        
        logger.LogDebug("Aggregated Data {Result}", mongoQuery.ToJsonString());
        
        
        // Works
        var summary = new SingleSeriesDataset() {
            Values = mongoQuery
                .GroupBy(q => q.Key.DecisionStatus ?? "TBC")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(k => k.Count)
                )
        };

        var r = new SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>()
        {
            Summary = summary,
            Result = mongoQuery.Select(a => new StringBucketDimensionResult()
                {
                    Fields = new Dictionary<string, string>()
                    {
                        { "Classification", a.Key.DecisionStatus ?? "TBC" },
                        { "CheckCode", a.Key.CheckCode! },
                        { "AlvsDecisionCode", a.Key.AlvsDecisionCode! },
                        { "BtmsDecisionCode", a.Key.BtmsDecisionCode! }
                    },
                    Value = a.Count
                })
                .OrderBy(r => r.Value)
                .Reverse()
                .ToList()
        };
        
            return Task.FromResult(r);
        
        // return DefaultSummarisedBucketResult();
    }
    
    // public Task<SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>> ByDecisionComplex(DateTime from, DateTime to)
    // {
    //     var mongoQuery = context
    //         .Movements
    //         // .Aggregate()
    //         .Where(m => m.CreatedSource >= from && m.CreatedSource < to)
    //         .Select(m => new
    //         {
    //             MovementInfo = new
    //             {
    //                 Id = m.Id,
    //                 UpdatedSource = m.UpdatedSource,
    //                 Updated = m.Updated,
    //                 Movement = m
    //             },
    //             // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
    //             // Get the most recent decision record from both systems
    //             AlvsDecision = (m.Decisions == null
    //                 ? null
    //                 : m.Decisions
    //                     .Where(d => d.ServiceHeader!.SourceSystem == "ALVS")
    //                     .OrderBy(d => d.ServiceHeader!.ServiceCalled)
    //                     .Reverse()
    //                     .FirstOrDefault())
    //                        // Creates a default item & check so we don't lose
    //                        // it in the selectmany below
    //                        ?? new CdsClearanceRequest()
    //                         {
    //                             Items = new []
    //                             {
    //                                 new Items()
    //                                 {
    //                                     Checks = new []
    //                                     {
    //                                         new Check()
    //                                         {
    //                                             CheckCode = "XXX",
    //                                             DecisionCode = "XXX"
    //                                         }
    //                                     }
    //                                 }
    //                             }
    //                         },
    //             BtmsDecision = m.Decisions == null
    //                 ? null
    //                 : m.Decisions
    //                     .Where(d => d.ServiceHeader!.SourceSystem == "BTMS")
    //                     .OrderBy(d => d.ServiceHeader!.ServiceCalled)
    //                     .Reverse()
    //                     .FirstOrDefault()
    //         })
    //         .SelectMany(m =>
    //             m.AlvsDecision!.Items!
    //                 .SelectMany(i =>
    //                     (i.Checks
    //                      ?? new[] { new Check() { CheckCode = "XXX", DecisionCode = "XXX" } })
    //                         .Select(c =>
    //                         new
    //                         {
    //                             m.MovementInfo,
    //                             AlvsDecisionInfo = c.CheckCode == "XXX" ? null : new
    //                             {
    //                                 Decision = m.AlvsDecision,
    //                                 DecisionNumber = m.AlvsDecision!.Header!.DecisionNumber,
    //                                 EntryVersion = m.AlvsDecision!.Header!.EntryVersionNumber,
    //                                 ItemNumber = i.ItemNumber,
    //                                 CheckCode = c.CheckCode,
    //                                 DecisionCode = c.DecisionCode,
    //                             },
    //                             BtmsDecisionInfo = new
    //                             {
    //                                 Decision = m.BtmsDecision,
    //                                 DecisionCode = m.BtmsDecision == null || m.BtmsDecision.Items == null
    //                                     ? null
    //                                     : m.BtmsDecision.Items!
    //                                         .First(bi => bi.ItemNumber == i.ItemNumber)
    //                                         .Checks!
    //                                         .First(ch => ch.CheckCode == c.CheckCode)
    //                                         .DecisionCode
    //                             }
    //                         }
    //                     )
    //                 )
    //                 .Select(a => new
    //                 {
    //                     a.MovementInfo,
    //                     a.AlvsDecisionInfo,
    //                     a.BtmsDecisionInfo,
    //                     Classification =
    //                         a.BtmsDecisionInfo == null ? "Btms Decision Not Present" :
    //                         a.AlvsDecisionInfo == null ? "Alvs Decision Not Present" :
    //                         
    //                         // TODO : we may want to try to consider clearance request version as well as the decision code
    //                         a.BtmsDecisionInfo.DecisionCode == a.AlvsDecisionInfo.DecisionCode ? "Btms Made Same Decision As Alvs" :
    //                         a.MovementInfo.Movement.Decisions
    //                             .Any(d => d.Header!.DecisionNumber == 1) ? "Alvs Decision Version 1 Not Present" : 
    //                         a.MovementInfo.Movement.ClearanceRequests
    //                             .Any(d => d.Header!.EntryVersionNumber == 1) ? "Alvs Clearance Request Version 1 Not Present" : 
    //                         a.AlvsDecisionInfo.DecisionNumber == 1 && a.AlvsDecisionInfo.EntryVersion == 1 ? "Single Entry And Decision Version" :
    //                         a.BtmsDecisionInfo.DecisionCode != a.AlvsDecisionInfo.DecisionCode ? "Btms Made Different Decision To Alvs" :
    //                         "Further Classification Needed"
    //                         // "FurtherClassificationNeeded Check Code Is " + a.AlvsDecisionInfo.CheckCode
    //                 })
    //             )
    //         
    //         // .Where(m => m.AlvsDecisionInfo == null)
    //         // .Where(m => m.AlvsDecision!.Items!.Any(i => i.Checks!.Any(c => c.CheckCode == "XXX")))
    //         .GroupBy(check => new
    //         {
    //             check.Classification,
    //             check.AlvsDecisionInfo!.CheckCode,
    //             AlvsDecisionCode = check.AlvsDecisionInfo!.DecisionCode,
    //             BtmsDecisionCode=check.BtmsDecisionInfo!.DecisionCode
    //         })
    //         .Select(g => new
    //         {
    //             g.Key, Count = g.Count()
    //         })
    //         
    //         .Execute(logger);
    //
    //     logger.LogDebug("Aggregated Data {Result}", mongoQuery.ToJsonString());
    //
    //     // Works
    //     var summary = new SingleSeriesDataset() {
    //         Values = mongoQuery
    //             .GroupBy(q => q.Key.Classification)
    //             .ToDictionary(
    //                 g => g.Key,
    //                 g => g.Sum(k => k.Count)
    //             )
    //     };
    //
    //     var r = new SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>()
    //     {
    //         Summary = summary,
    //         Result = mongoQuery.Select(a => new StringBucketDimensionResult()
    //         {
    //             Fields = new Dictionary<string, string>()
    //             {
    //                 { "Classification", a.Key.Classification },
    //                 { "CheckCode", a.Key.CheckCode! },
    //                 { "AlvsDecisionCode", a.Key.AlvsDecisionCode! },
    //                 { "BtmsDecisionCode", a.Key.BtmsDecisionCode! }
    //             },
    //             Value = a.Count
    //         })
    //         .OrderBy(r => r.Value)
    //         .Reverse()
    //         .ToList()
    //     };
    //     
    //     return Task.FromResult(r);
    // }

    private static Task<TabularDataset<ByNameDimensionResult>> DefaultTabularDatasetByNameDimensionResult()
    {
        return Task.FromResult(new TabularDataset<ByNameDimensionResult>()
        {
            Rows =
            [
                new TabularDimensionRow<ByNameDimensionResult>() { Key = "", Columns = [] }
            ]
        });
    }

    private static Task<SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>> DefaultSummarisedBucketResult()
    {
        return Task.FromResult(new SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>()
        {
            Summary = new SingleSeriesDataset()
            {
                Values = new Dictionary<string, int>()
            },
            Result = []
        });
    }

}
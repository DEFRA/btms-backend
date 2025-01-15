using System.Diagnostics.CodeAnalysis;
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

    public Task<SingleSeriesDataset> ByStatus(DateTime? from = null, DateTime? to = null)
    {
        var data = context
            .Movements
            .SelectLinkStatus()
            .Where(n => (from == null || n.CreatedSource >= from) && (to == null || n.CreatedSource < to))
            .GroupBy(m => m.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(g => g.Key, g => g.Count);
            
        var enumLookup = new JsonStringEnumConverterEx<LinkStatusEnum>();
        
        return Task.FromResult(new SingleSeriesDataset
        {
            Values = AnalyticsHelpers
                .GetMovementStatusSegments()
                .ToDictionary(status => enumLookup.GetValue(status), status => data.GetValueOrDefault(status, 0))
        });
    }

    public Task<MultiSeriesDataset<ByNumericDimensionResult>> ByItemCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .SelectLinkStatus()
            .GroupBy(m => new { LinkStatus = m.Status, ItemCount = m.Movement.Items.Count })
            .Select(g => new { g.Key.LinkStatus, g.Key.ItemCount, Count = g.Count() });
            
        var mongoResult = mongoQuery
            .Execute(logger)
            .ToList();
            
        var dictionary = mongoResult
            .ToDictionary(g => new { g.LinkStatus, g.ItemCount }, g => g.Count);
            
        var maxCount = mongoResult.Count > 0 ?
            mongoResult.Max(r => r.Count) : 0;

        var enumLookup = new JsonStringEnumConverterEx<LinkStatusEnum>();
        
        return Task.FromResult(new MultiSeriesDataset<ByNumericDimensionResult>()
        {
            Series = AnalyticsHelpers.GetMovementStatusSegments()
                .Select(status => new Series<ByNumericDimensionResult>()
                    {
                        Name = enumLookup.GetValue(status),
                        Dimension = "Item Count",
                        Results = Enumerable.Range(0, maxCount + 1)
                            .Select(i => new ByNumericDimensionResult
                            {
                                Dimension = i,
                                Value = dictionary.GetValueOrDefault(new { LinkStatus = status, ItemCount = i }, 0)
                            }).ToList()
                    }
                )
                .ToList()    
        });
    }
    
    public Task<MultiSeriesDataset<ByNumericDimensionResult>> ByUniqueDocumentReferenceCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .SelectLinkStatus()
            .GroupBy(m => new
            {
                LinkStatus = m.Status,
                DocumentReferenceCount = m.Movement.Items
                    .SelectMany(i => i.Documents == null ? new string[] {} : i.Documents.Select(d => d.DocumentReference))
                    .Distinct()
                    .Count()
            })
            .Select(g => new { g.Key.LinkStatus, g.Key.DocumentReferenceCount, MovementCount = g.Count() });
            
        var mongoResult = mongoQuery.Execute(logger).ToList();
        
        var dictionary = mongoResult
            .ToDictionary(
                g => new { LinkStatus = g.LinkStatus, g.DocumentReferenceCount },
                g => g.MovementCount);

        var maxReferences = mongoResult.Count > 0 ?
            mongoResult.Max(r => r.DocumentReferenceCount) : 0;

        var enumLookup = new JsonStringEnumConverterEx<LinkStatusEnum>();
        
        return Task.FromResult(new MultiSeriesDataset<ByNumericDimensionResult>()
        {
            Series = AnalyticsHelpers.GetMovementStatusSegments()
                .Select(status => new Series<ByNumericDimensionResult>()
                {
                    Name = enumLookup.GetValue(status),
                    Dimension = "Document Reference Count",
                    Results = Enumerable.Range(0, maxReferences + 1)
                        .Select(i => new ByNumericDimensionResult
                        {
                            Dimension = i,
                            Value = dictionary.GetValueOrDefault(new { LinkStatus = status, DocumentReferenceCount = i },
                                0)
                        }).ToList()
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
                    new AuditHistory(a, "ImportNotification", "import-notifications", n.Id!)
                )
            );
        
        var entries = movement!.AuditEntries
            .Select(a => new AuditHistory(a, "Movement", "movements", movementId))
            .Concat(notificationEntries);

        entries = entries.OrderBy(a => a.AuditEntry.CreatedSource);
        
        return new EntityDataset<AuditHistory>(entries);
    }

    public Task<SingleSeriesDataset> ByMaxVersion(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var data = context
            .Movements
            .WhereFilteredByCreatedDateAndParams(from, to, chedTypes, country)
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
    
    public Task<SingleSeriesDataset> ByMaxDecisionNumber(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var data = context
            .Movements
            .WhereFilteredByCreatedDateAndParams(from, to, chedTypes, country)
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

    public Task<List<ExceptionResult>> GetExceptions(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var movementExceptions = new MovementExceptions(context, logger);
        var (_, result) = movementExceptions
            .GetAllExceptions(from, to, false, chedTypes, country);
            
        return Task.FromResult(result);
    }
    
    public Task<SingleSeriesDataset> ExceptionSummary(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var movementExceptions = new MovementExceptions(context, logger);
        var (summary, _) = movementExceptions
            .GetAllExceptions(from, to, true, chedTypes, country);
            
        return Task.FromResult(summary);
    }

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
            .GetAggregatedRecordsDictionary(logger, filter, projection, group, datasetGroup, createDatasetName);

        var enumLookup = new JsonStringEnumConverterEx<LinkStatusEnum>();
        
        var output = AnalyticsHelpers.GetMovementStatusSegments()
            .Select(status => mongoResult.AsDataset(dateRange, enumLookup.GetValue(status)))
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
        DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var mongoQuery = context
            .Movements
            .WhereFilteredByCreatedDateAndParams(from, to, chedTypes, country)
            .SelectMany(d => d
                .AlvsDecisionStatus.Context.DecisionComparison!.Checks
                .Select(c => new
                {
                    Movement = d, Check = c,
                    //Add additional analysis before building it into the write time analysis
                    DecisionStatus = d.AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus ==
                                     DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs ? 
                                        DecisionStatusEnum.BtmsMadeSameDecisionAsAlvs :
                                        d.BtmsStatus.ChedTypes.Length > 1 ? DecisionStatusEnum.HasMultipleChedTypes :
                                        d.Relationships.Notifications.Data.Count > 1 ? DecisionStatusEnum.HasMultipleCheds :
                                        d.AlvsDecisionStatus.Context.DecisionComparison!.DecisionStatus
                }))
            .GroupBy(d => new
            {
                d.DecisionStatus,
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

        var enumLookup = new JsonStringEnumConverterEx<DecisionStatusEnum>();
        var summaryValues = mongoQuery
            .GroupBy(q => q.Key.DecisionStatus)
            .Select(g => new {g.Key, Sum = g.Sum(k => k.Count)})
            .OrderBy(s => s.Key)
            .ToDictionary(
                g => enumLookup.GetValue(g.Key),
                g => g.Sum
            );
        
        // Works
        var summary = new SingleSeriesDataset() {
            Values = summaryValues
        };
        
        var r = new SummarisedDataset<SingleSeriesDataset, StringBucketDimensionResult>()
        {
            Summary = summary,
            Result = mongoQuery.Select(a => new StringBucketDimensionResult()
                {
                    Fields = new Dictionary<string, string>()
                    {
                        { "Classification", enumLookup.GetValue(a.Key.DecisionStatus) },
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
    }

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
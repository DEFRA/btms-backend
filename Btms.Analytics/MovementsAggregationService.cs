using System.Collections;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Btms.Backend.Data;
using Btms.Common.Extensions;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using Btms.Model;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;

using Btms.Analytics.Extensions;

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
    public Task<MultiSeriesDatetimeDataset[]> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<Movement, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) => AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean());

        return Aggregate(dateRange, CreateDatasetName, matchFilter, "$createdSource", aggregateBy);
    }

    /// <summary>
    /// Aggregates movements by arrival date and returns counts by date period. Could be refactored to use a generic/interface in time
    /// </summary>
    /// <param name="from">Time period to search from (inclusive)</param>
    /// <param name="to">Time period to search to (exclusive)</param>
    /// <param name="aggregateBy">Aggregate by day/hour</param>
    /// <returns></returns>
    public Task<MultiSeriesDatetimeDataset[]> ByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<Movement, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) => AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean());

        return Aggregate(dateRange, CreateDatasetName, matchFilter, "$arrivesAt", aggregateBy);
    }

    public Task<SingeSeriesDataset> ByStatus(DateTime from, DateTime to)
    {
        var data = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => m.Relationships.Notifications.Data.Count > 0)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(g => AnalyticsHelpers.GetLinkedName(g.Key), g => g.Count);
            
        return Task.FromResult(new SingeSeriesDataset()
        {
            Values = AnalyticsHelpers.GetMovementSegments().ToDictionary(title => title, title => data.GetValueOrDefault(title, 0))
        });
    }

    public Task<MultiSeriesDataset[]> ByItemCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => new { Linked = m.Relationships.Notifications.Data.Count > 0, Items = m.Items.Count })
            .GroupBy(g => g.Key.Linked);

        var mongoResult = mongoQuery
            .Execute(logger)
            .SelectMany(g => g.Select(l => new
            {
                Linked = g.Key, 
                ItemCount = l.Key.Items,
                Count = l.Count()
            }))
            .ToList();
            
        var dictionary = mongoResult
            .ToDictionary(g => new { Title = AnalyticsHelpers.GetLinkedName(g.Linked), ItemCount = g.ItemCount }, g => g.Count);
            
        var maxCount = mongoResult
            .Max(r => r.Count);

        return Task.FromResult(AnalyticsHelpers.GetMovementSegments()
            .Select(title => new MultiSeriesDataset(title, "Item Count") {
                Results = Enumerable.Range(0, maxCount + 1)
                    .Select(i => new ByNumericDimensionResult()
                    {
                        Dimension = i,
                        Value = dictionary!.GetValueOrDefault(new { Title=title, ItemCount = i }, 0)
                    }).ToList()
            })
            .ToArray()    
        );
    }
    
    public Task<MultiSeriesDataset[]> ByUniqueDocumentReferenceCount(DateTime from, DateTime to)
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
            .GroupBy(g => g.Key.Linked);

        var mongoResult = mongoQuery
            .Execute(logger)
            .SelectMany(g => g.Select(l => new
                {
                    Linked = g.Key,
                    DocumentReferenceCount = l.Key.DocumentReferenceCount,
                    MovementCount = l.Count()
                }))
            .ToList();
        
        var dictionary = mongoResult
            .ToDictionary(
                g => new { Title = AnalyticsHelpers.GetLinkedName(g.Linked), DocumentReferenceCount = g.DocumentReferenceCount },
                g => g.MovementCount)!;

        var maxReferences = mongoResult.Max(r => r.DocumentReferenceCount);
        
        return Task.FromResult(AnalyticsHelpers.GetMovementSegments()
            .Select(title => new MultiSeriesDataset(title, "Document Reference Count") {
                Results = Enumerable.Range(0, maxReferences + 1)
                    .Select(i => new ByNumericDimensionResult()
                    {
                        Dimension = i,
                        Value = dictionary!.GetValueOrDefault(new { Title=title, DocumentReferenceCount = i }, 0)
                    }).ToList()
            })
            .ToArray()    
        );
    }

    public Task<SingeSeriesDataset> UniqueDocumentReferenceByMovementCount(DateTime from, DateTime to)
    {
        var mongoQuery = context
            .Movements
            .Where(m => m.CreatedSource >= from && m.CreatedSource < to)
            .SelectMany(m => m.Items.Select(i => new { Item = i, MovementId = m.Id }))
            .SelectMany(i => i.Item.Documents!.Select(d =>
                new { MovementId = i.MovementId, DocumentReference = d.DocumentReference }))
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

            var result = new SingeSeriesDataset() { Values = mongoResult };
            
            return Task.FromResult(result);
    }

    private Task<MultiSeriesDatetimeDataset[]> Aggregate(DateTime[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<Movement, bool>> filter, string dateField, AggregationPeriod aggregateBy)
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
        
        logger.LogDebug("Aggregated Data {result}", output.ToList().ToJsonString());
        
        return Task.FromResult(output);
    }
}
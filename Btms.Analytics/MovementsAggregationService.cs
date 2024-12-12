using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Btms.Analytics.Extensions;
using Btms.Backend.Data;
using Btms.Model.Extensions;
using Btms.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using Btms.Analytics.Extensions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

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

    public Task<SingeSeriesDataset> ByStatus(DateTime from, DateTime to)
    {
        var data = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => m.Relationships.Notifications.Data.Count > 0)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(g => AnalyticsHelpers.GetLinkedName(g.Key), g => g.Count);
            
        return Task.FromResult(new SingeSeriesDataset
        {
            Values = AnalyticsHelpers.GetMovementSegments().ToDictionary(title => title, title => data.GetValueOrDefault(title, 0))
        });
    }

    public Task<MultiSeriesDataset[]> ByItemCount(DateTime from, DateTime to)
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

        return Task.FromResult(AnalyticsHelpers.GetMovementSegments()
            .Select(title => new MultiSeriesDataset(title, "Item Count") {
                Results = Enumerable.Range(0, maxCount + 1)
                    .Select(i => new ByNumericDimensionResult
                    {
                        Dimension = i,
                        Value = dictionary.GetValueOrDefault(new { Title=title, ItemCount = i }, 0)
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
            .Select(g => new { g.Key.Linked, g.Key.DocumentReferenceCount, MovementCount = g.Count() });
            
        var mongoResult = mongoQuery.Execute(logger).ToList();
        
        var dictionary = mongoResult
            .ToDictionary(
                g => new { Title = AnalyticsHelpers.GetLinkedName(g.Linked), g.DocumentReferenceCount },
                g => g.MovementCount);

        var maxReferences = mongoResult.Count > 0 ?
            mongoResult.Max(r => r.DocumentReferenceCount) : 0;
        
        return Task.FromResult(AnalyticsHelpers.GetMovementSegments()
            .Select(title => new MultiSeriesDataset(title, "Document Reference Count") {
                Results = Enumerable.Range(0, maxReferences + 1)
                    .Select(i => new ByNumericDimensionResult
                    {
                        Dimension = i,
                        Value = dictionary.GetValueOrDefault(new { Title=title, DocumentReferenceCount = i }, 0)
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

            var result = new SingeSeriesDataset { Values = mongoResult };
            
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
        
        logger.LogDebug("Aggregated Data {Result}", output.ToList().ToJsonString());
        
        return Task.FromResult(output);
    }
}
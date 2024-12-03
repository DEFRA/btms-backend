using System.Collections;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Cdms.Backend.Data;
using Cdms.Common.Extensions;
using Cdms.Model.Extensions;
using Cdms.Model.Ipaffs;
using Cdms.Model;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cdms.Analytics;

public class LinkingAggregationService(IMongoDbContext context, ILogger<LinkingAggregationService> logger) : ILinkingAggregationService
{
    static DateTime AggregateDateCreator(BsonValue b) => b["dateToUse"].BsonType != BsonType.Null ? b["dateToUse"].ToUniversalTime() : DateTime.MinValue;
    
    private static string GetLinkedName(bool linked, string type)
    {
        return $"{type} {GetLinkedName(linked)}";
    }
    private static string GetLinkedName(bool linked)
    {
        return linked ? "Linked" : "Not Linked";
    }
    
    /// <summary>
    /// Aggregates movements by createdSource and returns counts by date period. Could be refactored to use a generic/interface in time
    /// </summary>
    /// <param name="from">Time period to search from (inclusive)</param>
    /// <param name="to">Time period to search to (exclusive)</param>
    /// <param name="aggregateBy">Aggregate by day/hour</param>
    /// <returns></returns>
    public Task<Dataset[]> MovementsByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        int RangeFromPeriod(TimeSpan t) => Convert.ToInt32(aggregateBy == AggregationPeriod.Hour ? t.TotalHours : t.TotalDays);
        DateTime IncrementPeriod(DateTime d, int offset) => aggregateBy == AggregationPeriod.Hour ? d.AddHours(offset) : d.AddDays(offset);

        // By creating the dates we care about, we can ensure the arrays have all the dates, 
        // including any series that don't have data on a given day. We need these to be zero for the chart to render
        // correctly
        var dateRange = Enumerable.Range(0, RangeFromPeriod((to - from))).Reverse()
            .Select(offset => IncrementPeriod(from, offset)) // from.AddDays(offset))
            .ToArray(); 
        
        Expression<Func<Movement, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) => GetLinkedName(b["_id"]["linked"].ToBoolean());
        
        return GetMovementAggregate(dateRange, CreateDatasetName, matchFilter, AggregateDateCreator, "$createdSource", aggregateBy);
    }

    /// <summary>
    /// Aggregates movements by arrival date and returns counts by date period. Could be refactored to use a generic/interface in time
    /// </summary>
    /// <param name="from">Time period to search from (inclusive)</param>
    /// <param name="to">Time period to search to (exclusive)</param>
    /// <param name="aggregateBy">Aggregate by day/hour</param>
    /// <returns></returns>
    public Task<Dataset[]> MovementsByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        int RangeFromPeriod(TimeSpan t) => Convert.ToInt32(aggregateBy == AggregationPeriod.Hour ? t.TotalHours : t.TotalDays);
        DateTime IncrementPeriod(DateTime d, int offset) => aggregateBy == AggregationPeriod.Hour ? d.AddHours(offset) : d.AddDays(offset);

        // By creating the dates we care about, we can ensure the arrays have all the dates, 
        // including any series that don't have data on a given day. We need these to be zero for the chart to render
        // correctly
        var dateRange = Enumerable.Range(0, RangeFromPeriod((to - from))).Reverse()
            .Select(offset => IncrementPeriod(from, offset)) // from.AddDays(offset))
            .ToArray(); 
        
        Expression<Func<Movement, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) => GetLinkedName(b["_id"]["linked"].ToBoolean());

        return GetMovementAggregate(dateRange, CreateDatasetName, matchFilter, AggregateDateCreator, "$arrivesAt", aggregateBy);
    }
    
    public Task<Dataset[]> ImportNotificationsByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        int RangeFromPeriod(TimeSpan t) => Convert.ToInt32(aggregateBy == AggregationPeriod.Hour ? t.TotalHours : t.TotalDays);
        DateTime IncrementPeriod(DateTime d, int offset) => aggregateBy == AggregationPeriod.Hour ? d.AddHours(offset) : d.AddDays(offset);

        // By creating the dates we care about, we can ensure the arrays have all the dates, 
        // including any series that don't have data on a given day. We need these to be zero for the chart to render
        // correctly
        var dateRange = Enumerable.Range(0, RangeFromPeriod((to - from))).Reverse()
            .Select(offset => IncrementPeriod(from, offset)) // from.AddDays(offset))
            .ToArray(); 
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) => GetLinkedName(b["_id"]["linked"].ToBoolean(), b["_id"]["importNotificationType"].ToString()!);
        
        return GetImportNotificationAggregate(dateRange, CreateDatasetName, matchFilter, AggregateDateCreator, "$createdSource", aggregateBy);
    }

    public Task<Dataset[]> ImportNotificationsByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        // By creating the dates we care about, we can ensure the arrays have all the dates, 
        // including any series that don't have data on a given day. We need these to be zero for the chart to render
        // correctly
        var dateRange = Enumerable.Range(0, Convert.ToInt32((to - from).TotalDays)).Reverse()
            .Select(offset => from.AddDays(offset))
            .ToArray();
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.PartOne!.ArrivesAt >= from && n.PartOne!.ArrivesAt < to;

        string CreateDatasetName(BsonDocument b) => $"{b["_id"]["importNotificationType"].ToString()!} {(b["_id"]["linked"].ToBoolean() ? "Linked" : "Not Linked")}";
        
        return GetImportNotificationAggregate(dateRange, CreateDatasetName, matchFilter, AggregateDateCreator, "$partOne.arrivesAt", aggregateBy);
    }

    public Task<PieChartDataset> MovementsByStatus(DateTime from, DateTime to)
    {
        var data = context
            .Movements
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(m => m.Relationships.Notifications.Data.Count > 0 )
            .Select(g => new { g.Key, Count = g.Count() })
            .ToList();

        return Task.FromResult(new PieChartDataset()
        {
            Values = data.ToDictionary(g => GetLinkedName(g.Key), g=> g.Count )
        });
    }
    
    public Task<PieChartDataset> ImportNotificationsByStatus(DateTime from, DateTime to)
    {
        var data = context
            .Notifications
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(n => new { n.ImportNotificationType, Linked = n.Relationships.Movements.Data.Count > 0 })
            .Select(g => new { g.Key.Linked, g.Key.ImportNotificationType, Count = g.Count() })
            .ToList();

        return Task.FromResult(new PieChartDataset()
        {
            Values = data.ToDictionary(g => GetLinkedName(g.Linked, g.ImportNotificationType.ToString()!), g=> g.Count )
        });
    }

    private Task<Dataset[]> GetImportNotificationAggregate(DateTime[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<ImportNotification, bool>> filter, Func<BsonValue, DateTime> aggregateDateCreator, string dateField, AggregationPeriod aggregateBy)
    {
        var comparer = Comparer<ByDateTimeResult>.Create((d1, d2) => d1.Period.CompareTo(d2.Period));

        var truncateBy = aggregateBy == AggregationPeriod.Hour ? "hour" : "day";
        
        ProjectionDefinition<ImportNotification> projection = "{linked:{ $ne: [0, { $size: '$relationships.movements.data'}]}, 'importNotificationType':1, dateToUse: { $dateTrunc: { date: '" + dateField + "', unit: '" + truncateBy + "'}}}";
        
        // First aggregate the dataset by chedtype, whether its matched and the date it arrives. Count the number in each bucket.
        ProjectionDefinition<BsonDocument> group = "{_id: { linked: '$linked', importNotificationType: '$importNotificationType', dateToUse: '$dateToUse' }, count: { $count: { } }}";
        
        // Then further group by chedtype & whether its matched to give us the structure we need in our chart
        ProjectionDefinition<BsonDocument> datasetGroup = "{_id: { importNotificationType: '$_id.importNotificationType', linked: '$_id.linked'}, dates: { $push: { dateToUse: '$_id.dateToUse', count: '$count' }}}";

        var mongoResult1 = context
            .Notifications
            .Aggregate()
            .Match(filter)
            .Project(projection)
            .Group(group)
            .Group(datasetGroup)
            .ToList();
            
        var mongoResult = mongoResult1
            .Select(b =>
                {
                    // Turn the date : count aggregation into a dictionary 
                    var dates = b["dates"].AsBsonArray
                        .ToDictionary(aggregateDateCreator, d => d["count"].AsInt32);

                    // Then map it into a list of the dates we want in our range 
                    return new Dataset(createDatasetName(b))
                    {
                        Periods = dateRange.Select(resultDate => new ByDateTimeResult() { Period = resultDate, Value = dates.GetValueOrDefault(resultDate, 0)})
                            .Order(comparer)
                            .ToList()
                    };
                }
            )
            .OrderBy(d => d.Name)
            .ToArray();
        
        logger.LogDebug("Aggregated Data {Result}", mongoResult.ToList().ToJsonString());
        
        return Task.FromResult(mongoResult);
    }
    
    private Task<Dataset[]> GetMovementAggregate(DateTime[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<Movement, bool>> filter, Func<BsonValue, DateTime> aggregateDateCreator, string dateField, AggregationPeriod aggregateBy)
    {
        var comparer = Comparer<ByDateTimeResult>.Create((d1, d2) => d1.Period.CompareTo(d2.Period));

        var truncateBy = aggregateBy == AggregationPeriod.Hour ? "hour" : "day";
        
        ProjectionDefinition<Movement> projection = "{linked:{ $ne: [0, { $size: '$relationships.notifications.data'}]}, dateToUse: { $dateTrunc: { date: '" + dateField + "', unit: '" + truncateBy + "'}}}";
        
        // First aggregate the dataset by whether its matched and the date it arrives. Count the number in each bucket.
        ProjectionDefinition<BsonDocument> group = "{_id: { linked: '$linked', dateToUse: '$dateToUse' }, count: { $count: { } }}";
        
        // Then further group by whether its matched and get the count to give us the structure we need in our chart
        ProjectionDefinition<BsonDocument> datasetGroup = "{_id: { linked: '$_id.linked'}, dates: { $push: { dateToUse: '$_id.dateToUse', count: '$count' }}}";

        var mongoResult1 = context
            .Movements
            .Aggregate()
            .Match(filter)
            .Project(projection)
            .Group(group)
            .Group(datasetGroup)
            .ToList();
            
        var mongoResult = mongoResult1
            .Select(b =>
                {
                    // Turn the date : count aggregation into a dictionary 
                    var dates = b["dates"].AsBsonArray
                        .ToDictionary(aggregateDateCreator, d => d["count"].AsInt32);

                    // Then map it into a list of the dates we want in our range 
                    return new Dataset(createDatasetName(b))
                    {
                        Periods = dateRange.Select(resultDate => new ByDateTimeResult() { Period = resultDate, Value = dates.GetValueOrDefault(resultDate, 0)})
                            .Order(comparer)
                            .ToList()
                    };
                }
            )
            .OrderBy(d => d.Name)
            .ToArray();
        
        logger.LogDebug("Aggregated Data {Result}", mongoResult.ToList().ToJsonString());
        
        return Task.FromResult(mongoResult);
    }
}
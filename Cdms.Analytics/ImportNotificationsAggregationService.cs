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

using Cdms.Analytics.Extensions;

namespace Cdms.Analytics;

public class ImportNotificationsAggregationService(IMongoDbContext context, ILogger<ImportNotificationsAggregationService> logger) : IImportNotificationsAggregationService
{
    public Task<Dataset[]> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) =>
            AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean(), b["_id"]["importNotificationType"].ToString()!.FromImportNotificationTypeEnumString()!);

        return Aggregate(dateRange, CreateDatasetName, matchFilter, "$createdSource", aggregateBy);
    }

    public Task<Dataset[]> ByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.PartOne!.ArrivesAt >= from && n.PartOne!.ArrivesAt < to;

        string CreateDatasetName(BsonDocument b) =>
            AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean(), b["_id"]["importNotificationType"].ToString()!.FromImportNotificationTypeEnumString()!);

        return Aggregate(dateRange, CreateDatasetName, matchFilter, "$partOne.arrivesAt", aggregateBy);
    }
    
    public Task<PieChartDataset> ByStatus(DateTime from, DateTime to)
    {
        var data = context
            .Notifications
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(n => new { n.ImportNotificationType, Linked = n.Relationships.Movements.Data.Count > 0 })
            .Select(g => new { g.Key.Linked, g.Key.ImportNotificationType, Count = g.Count() })
            .ToDictionary(g => AnalyticsHelpers.GetLinkedName(g.Linked, g.ImportNotificationType.AsString()!),
                g => g.Count);

        return Task.FromResult(new PieChartDataset()
        {
            Values = GetSegments().ToDictionary(title => title, title => data.GetValueOrDefault(title, 0))
        });
    }

    private static string[] GetSegments()
    {
        return ModelHelpers.GetChedTypes()
            .SelectMany(chedType => new string[] { $"{chedType} Linked", $"{chedType} Not Linked" })
            .ToArray();
    }

    private Task<Dataset[]> Aggregate(DateTime[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<ImportNotification, bool>> filter, string dateField, AggregationPeriod aggregateBy)
    {
        var truncateBy = aggregateBy == AggregationPeriod.Hour ? "hour" : "day";
        
        ProjectionDefinition<ImportNotification> projection = "{linked:{ $ne: [0, { $size: '$relationships.movements.data'}]}, 'importNotificationType':1, dateToUse: { $dateTrunc: { date: '" + dateField + "', unit: '" + truncateBy + "'}}}";
        
        // First aggregate the dataset by chedtype, whether its matched and the date it arrives. Count the number in each bucket.
        ProjectionDefinition<BsonDocument> group = "{_id: { linked: '$linked', importNotificationType: '$importNotificationType', dateToUse: '$dateToUse' }, count: { $count: { } }}";
        
        // Then further group by chedtype & whether its matched to give us the structure we need in our chart
        ProjectionDefinition<BsonDocument> datasetGroup = "{_id: { importNotificationType: '$_id.importNotificationType', linked: '$_id.linked'}, dates: { $push: { dateToUse: '$_id.dateToUse', count: '$count' }}}";

        var mongoResult = context
            .Notifications
            .GetAggregatedRecordsDictionary(filter, projection, group, datasetGroup, createDatasetName);
        
        var output = GetSegments()
            .Select(title => mongoResult.AsDataset(dateRange, title))
            .AsOrderedArray();
        
        logger.LogDebug("Aggregated Data {result}", output.ToList().ToJsonString());
        
        return Task.FromResult(output);
    }
}
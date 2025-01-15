using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Btms.Backend.Data;
using Btms.Model.Extensions;
using Btms.Model.Ipaffs;
using MongoDB.Bson;
using MongoDB.Driver;

using Btms.Analytics.Extensions;
using Btms.Model.Auditing;
using MongoDB.Driver.Linq;

namespace Btms.Analytics;

public class ImportNotificationsAggregationService(IMongoDbContext context, ILogger<ImportNotificationsAggregationService> logger) 
    : IImportNotificationsAggregationService
{
    public Task<MultiSeriesDatetimeDataset> ByCreated(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.CreatedSource >= from && n.CreatedSource < to;

        string CreateDatasetName(BsonDocument b) =>
            AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean(), b["_id"]["importNotificationType"].ToString()!.FromImportNotificationTypeEnumString());

        return AggregateByLinkedAndNotificationType(dateRange, CreateDatasetName, matchFilter, "$createdSource", aggregateBy);
    }

    public Task<MultiSeriesDatetimeDataset> ByArrival(DateTime from, DateTime to, AggregationPeriod aggregateBy = AggregationPeriod.Day)
    {
        var dateRange = AnalyticsHelpers.CreateDateRange(from, to, aggregateBy);
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.PartOne!.ArrivesAt >= from && n.PartOne!.ArrivesAt < to;

        string CreateDatasetName(BsonDocument b) =>
            AnalyticsHelpers.GetLinkedName(b["_id"]["linked"].ToBoolean(), b["_id"]["importNotificationType"].ToString()!.FromImportNotificationTypeEnumString());

        return AggregateByLinkedAndNotificationType(dateRange, CreateDatasetName, matchFilter, "$partOne.arrivesAt", aggregateBy);
    }
    
    public Task<SingleSeriesDataset> ByStatus(DateTime? from = null, DateTime? to = null)
    {
        var data = context
            .Notifications
            .Where(n => (from == null || n.CreatedSource >= from) && (to == null || n.CreatedSource < to))
            .GroupBy(n => new { n.ImportNotificationType, Linked = n.Relationships.Movements.Data.Count > 0 })
            .Select(g => new { g.Key.Linked, g.Key.ImportNotificationType, Count = g.Count() })
            .ToDictionary(g => AnalyticsHelpers.GetLinkedName(g.Linked, g.ImportNotificationType.AsString()),
                g => g.Count);

        return Task.FromResult(new SingleSeriesDataset
        {
            Values = AnalyticsHelpers.GetImportNotificationSegments().ToDictionary(title => title, title => data.GetValueOrDefault(title, 0))
        });
    }
    
    public EntityDataset<ScenarioItem> Scenarios(DateTime? from = null, DateTime? to = null)
    {
        var commodityNotifications = context
            .Notifications
            .Where(n => (
                            from == null || n.CreatedSource >= from) &&
                        (to == null || n.CreatedSource < to) &&
                        n.Relationships.Movements.Data.Any()
            )
            .SelectMany(n => n
                .Commodities
                .Select(c => new
                {
                    Ched = n.Id!,
                    Mrns = n.Relationships.Movements.Data.Select(m => m.Id),
                    CommodityDescription = c.CommodityDescription!,
                    CommodityCount = n.Commodities.Count(),
                    MovementCount = n.Relationships.Movements.Data.Count,
                    // CommodityDescriptionLCase = c.CommodityDescription!.ToLower(), 
                    Detail = new { Notification = n, Commodity = c }
                }));
        
        var sweetPeppers = commodityNotifications
            .Where(c =>
                c.Detail.Commodity.CommodityId == "07096010" &&
                c.MovementCount > 3
                // c.CommodityDescription == "Sweet peppers"
                // c.CommodityDescriptionLCase.Contains("sweet") && 
                // c.CommodityDescriptionLCase.Contains("peppers") && 
                // !c.CommodityDescriptionLCase.Contains("other than sweet")
            )
            .Take(5)
            .Execute(logger);

        var data = sweetPeppers
            .Select(s => new ScenarioItem()
            {
                Scenario = "Sweet Peppers", Keys = s.Mrns.Concat([s.Ched]).ToArray()!
            })
            .ToList();

        return new EntityDataset<ScenarioItem>(data);
    }

    public Task<MultiSeriesDataset<ByNumericDimensionResult>> ByCommodityCount(DateTime from, DateTime to)
    {
        var query = context
            .Notifications
            .Where(n => n.CreatedSource >= from && n.CreatedSource < to)
            .GroupBy(n => new
            {
                ImportNotificationType = n.ImportNotificationType!.Value,
                Linked = n.Relationships.Movements.Data.Count > 0,
                
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                // This is not nullable, but is being represented as null in mongo and so this linq
                // query needs to consider the null
                CommodityCount = n.Commodities == null ? 0 : n.Commodities.Count()
            })
            .Select(g => new { g.Key, Count = g.Count() });

        var result = query
            .Execute(logger)
            .GroupBy(r => new { r.Key.ImportNotificationType, r.Key.Linked })
            .ToList();
        
        var maxCommodities = result.Count > 0 ?
            result.Max(r => r.Any() ? r.Max(i => i.Key.CommodityCount) : 0) : 0;
        
        var list = result
            .SelectMany(g =>
                g.Select(r =>
                    new {
                        Title = AnalyticsHelpers.GetLinkedName(g.Key.Linked, g.Key.ImportNotificationType.AsString()),
                        r.Key.CommodityCount,
                        NotificationCount = r.Count
                    })
            )
            .ToList();

        var asDictionary = list
            .ToDictionary(
                g => new { g.Title, g.CommodityCount },
                g => g.NotificationCount);
        
        
        return Task.FromResult(new MultiSeriesDataset<ByNumericDimensionResult>()
        {
            Series = AnalyticsHelpers.GetImportNotificationSegments()
                .Select(title => new Series<ByNumericDimensionResult>()
                {
                    Name = title,
                    Dimension = "ItemCount",
                    Results = Enumerable.Range(0, maxCommodities + 1)
                        .Select(i => new ByNumericDimensionResult
                        {
                            Dimension = i,
                            Value = asDictionary.GetValueOrDefault(new { Title=title, CommodityCount = i })
                        }).ToList()
                })
                .ToList()
        });
    }

    public Task<SingleSeriesDataset> ByMaxVersion(DateTime from, DateTime to, ImportNotificationTypeEnum[]? chedTypes = null, string? country = null)
    {
        var data = context
            .Notifications
            .WhereFilteredByCreatedDateAndParams(from, to, chedTypes, country)
            .GroupBy(n => new { MaxVersion =
                n.AuditEntries.Where(a => a.CreatedBy == CreatedBySystem.Ipaffs).Max(a => a.Version )
            })
            .Select(g => new { MaxVersion = g.Key.MaxVersion ?? 0, Count = g.Count() })
            .ExecuteAsSortedDictionary(logger, g => g.MaxVersion, g => g.Count);

        return Task.FromResult(new SingleSeriesDataset
        {
            Values = data.ToDictionary(d => d.Key.ToString(), d => d.Value)
        });
    }

    private Task<MultiSeriesDatetimeDataset> AggregateByLinkedAndNotificationType(DateTime[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<ImportNotification, bool>> filter, string dateField, AggregationPeriod aggregateBy)
    {
        var truncateBy = aggregateBy == AggregationPeriod.Hour ? "hour" : "day";
        
        ProjectionDefinition<ImportNotification> projection = "{linked:{ $ne: [0, { $size: '$relationships.movements.data'}]}, 'importNotificationType':1, dateToUse: { $dateTrunc: { date: '" + dateField + "', unit: '" + truncateBy + "'}}}";
        
        // First aggregate the dataset by chedtype, whether its matched and the date it arrives. Count the number in each bucket.
        ProjectionDefinition<BsonDocument> group = "{_id: { linked: '$linked', importNotificationType: '$importNotificationType', dateToUse: '$dateToUse' }, count: { $count: { } }}";
        
        // Then further group by chedtype & whether its matched to give us the structure we need in our chart
        ProjectionDefinition<BsonDocument> datasetGroup = "{_id: { importNotificationType: '$_id.importNotificationType', linked: '$_id.linked'}, dates: { $push: { dateToUse: '$_id.dateToUse', count: '$count' }}}";

        var mongoResult = context
            .Notifications
            .GetAggregatedRecordsDictionary(logger, filter, projection, group, datasetGroup, createDatasetName);
        
        var output = AnalyticsHelpers.GetImportNotificationSegments()
            .Select(title => mongoResult.AsDataset(dateRange, title))
            .AsOrderedArray(d => d.Name);
        
        logger.LogDebug("Aggregated Data {Result}", output.ToList().ToJsonString());
        
        return Task.FromResult(new MultiSeriesDatetimeDataset() { Series = output.ToList() });
    }
}
using System.Collections;
using Microsoft.Extensions.Logging;
using System.Data.Entity;
using System.Linq.Expressions;
using Cdms.Backend.Data;
using Cdms.Common.Extensions;
using Cdms.Model.Extensions;
using Cdms.Model.Ipaffs;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cdms.Analytics;

public class MatchingAggregationService(IMongoDbContext context, ILogger<MatchingAggregationService> logger) : IMatchingAggregationService
{
    public Task<Dataset[]> GetImportNotificationLinkingByCreated()
    {
        var days = 30;
        
        // By creating the dates we care about, we can ensure the arrays have all the dates, 
        // including any series that don't have data on a given day. We need these to be zero for the chart to render
        // correctly
        var dateRange = Enumerable.Range(0, days).Reverse()
            .Select(offset => DateTime.Today.AddDays(-offset).ToDate())
            .ToArray(); 
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.CreatedSource >= DateTime.Today.AddDays(-days) && n.CreatedSource <= DateTime.Today;

        Func<BsonDocument, string> createDatasetName = b =>
            $"{b["_id"]["importNotificationType"].ToString()!} {(b["_id"]["linked"].ToBoolean() ? "Linked" : "Not Linked")}";
        
        Func<BsonValue, DateOnly> aggregateDateCreator = b =>
            b["dateToUse"].ToUniversalTime().ToDate();
        
        return GetImportNotificationAggregate(dateRange, createDatasetName, matchFilter, aggregateDateCreator, "$createdSource");
    }

    public Task<Dataset[]> GetImportNotificationLinkingByArrival()
    {
        var days = 30;
        
        // By creating the dates we care about, we can ensure the arrays have all the dates, 
        // including any series that don't have data on a given day. We need these to be zero for the chart to render
        // correctly
        var dateRange = Enumerable.Range(0, days)
            .Select(offset => DateTime.Today.AddDays(offset).ToDate())
            .ToArray(); 
        
        Expression<Func<ImportNotification, bool>> matchFilter = n =>
            n.PartOne!.ArrivesAt >= DateTime.Today && n.PartOne!.ArrivesAt <= DateTime.Today.AddDays(days);

        Func<BsonDocument, string> createDatasetName = b =>
            $"{b["_id"]["importNotificationType"].ToString()!} {(b["_id"]["linked"].ToBoolean() ? "Linked" : "Not Linked")}";
        
        Func<BsonValue, DateOnly> aggregateDateCreator = b =>
            b["dateToUse"].ToUniversalTime().ToDate();
        
        return GetImportNotificationAggregate(dateRange, createDatasetName, matchFilter, aggregateDateCreator, "$partOne.arrivesAt");
    }
    
    private Task<Dataset[]> GetImportNotificationAggregate(DateOnly[] dateRange, Func<BsonDocument, string> createDatasetName, Expression<Func<ImportNotification, bool>> filter, Func<BsonValue, DateOnly> aggregateDateCreator, string dateField)
    {
        var comparer = Comparer<ByDateResult>.Create((d1, d2) => d1.Date.CompareTo(d2.Date));
        
        ProjectionDefinition<ImportNotification> projection = "{linked:{ $ne: [0, { $size: '$relationships.movements.data'}]}, 'importNotificationType':1, dateToUse: { $dateTrunc: { date: '" + dateField + "', unit: 'day'}}}";
        
        // First aggregate the dataset by chedtype, whether its matched and the date it arrives. Count the number in each bucket.
        ProjectionDefinition<BsonDocument> group = "{_id: { linked: '$linked', importNotificationType: '$importNotificationType', dateToUse: '$dateToUse' }, count: { $count: { } }}";
        
        // Then further group by chedtype & whether its matched to give us the structure we need in our chart
        ProjectionDefinition<BsonDocument> datasetGroup = "{_id: { importNotificationType: '$_id.importNotificationType', linked: '$_id.linked'}, dates: { $push: { dateToUse: '$_id.dateToUse', count: '$count' }}}";
        
        var mongoResult = context
            .Notifications
            .Aggregate()
            .Match(filter)
            .Project(projection)
            .Group(group)
            .Group(datasetGroup)
            .ToList()
            .Select(b =>
                {
                    // Turn the date : count aggregation into a dictionary 
                    var dates = b["dates"].AsBsonArray
                        .ToDictionary(aggregateDateCreator, d => d["count"].AsInt32);

                    // Then map it into a list of the dates we want in our range 
                    return new Dataset(createDatasetName(b))
                    {
                        Dates = dateRange.Select(resultDate => new ByDateResult() { Date = resultDate, Value = dates.GetValueOrDefault(resultDate, 0)})
                            .Order(comparer)
                            .ToList()
                    };
                }
            )
            .OrderBy(d => d.Name)
            .ToArray();
        
        logger.LogDebug("Aggregated Data {result}", mongoResult.ToList().ToJsonString());
        
        return Task.FromResult(mongoResult);
    }
}
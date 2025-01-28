using Btms.Model.Ipaffs;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Primitives;

namespace Btms.Backend.JsonApi;

// ReSharper disable once ClassNeverInstantiated.Global
public class ImportNotificationJsonApiResourceDefinition(IResourceGraph resourceGraph)
    : JsonApiResourceDefinition<ImportNotification, string?>(resourceGraph)
{
    private const string RelationshipUpdated = "relationshipUpdated";
    private const string GenericError = $"Invalid '{RelationshipUpdated}' query value";

    public override QueryStringParameterHandlers<ImportNotification>
        OnRegisterQueryableHandlersForQueryStringParameters() =>
        new() { [RelationshipUpdated] = HandleRelationshipUpdated, };

    private static IQueryable<ImportNotification> HandleRelationshipUpdated(IQueryable<ImportNotification> source, StringValues parameterValue)
    {
        // The existence of a parameterValue will be validated by JsonApiDotNetCore
        // before we hit our own validation logic
        var value = parameterValue.ToString();
        
        var range = value.Split(",");
        if (range.Length != 2)
            throw new InvalidQueryStringParameterException(RelationshipUpdated,
                GenericError,
                $"Expected format is two UTC dates separated by a comma, but was '{value}'");

        var from = ParseDateTime(range[0]);
        var to = ParseDateTime(range[1]);
        
        EnsureUtc(nameof(from), from);
        EnsureUtc(nameof(to), to);

        return source.Where(x => x.Relationships.Movements.Data.Any(y => y.Updated >= from && y.Updated < to));
    }

    private static DateTime ParseDateTime(string value)
    {
        try
        {
            return (DateTime) RuntimeTypeConverter.ConvertType(value, typeof(DateTime))!;
        }
        catch (FormatException formatException)
        {
            throw new InvalidQueryStringParameterException(RelationshipUpdated,
                GenericError,
                $"Expected DateTime, but was '{value}'", 
                formatException);
        }
    }

    private static void EnsureUtc(string type, DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new InvalidQueryStringParameterException(RelationshipUpdated,
                GenericError,
                $"Expected {type} date as UTC, but was '{value:O}'");
    }
}
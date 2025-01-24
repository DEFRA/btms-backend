using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Common.Extensions;
using Btms.Model.Cds;
using MongoDB.Bson.Serialization.Attributes;

namespace Btms.Model.Auditing;

[BsonKnownTypes(typeof(DecisionContext), typeof(CdsFinalisation))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(DecisionContext), nameof(DecisionContext))]
[JsonDerivedType(typeof(CdsFinalisation), nameof(CdsFinalisation))]
public abstract class AuditContext
{
    
}

/// <summary>
/// Serialise the derived types of AuditContext
/// </summary>
/// <typeparam name="TType"></typeparam>
public class DecisionContextConverter<TType> : JsonConverter<TType> where TType : AuditContext
{
    // private JsonConverter? _auditContextConverter;
    
    [return: MaybeNull]
    public override TType Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // if (typeToConvert is AuditContext)
        // {
        //     
        // }
        //
        var converters = options
            .Converters
            // .Where(c => c is not DecisionContextConverter<TType>)
            .ToList();
        //
        Console.WriteLine("DecisionContextConverter {0}", typeToConvert.FullName);

        var newOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = options.PropertyNamingPolicy
        };

        if (typeToConvert == typeof(AuditContext))
        {
            // We only want the converter present once, otherwise we get a stack overflow
            var auditContextConverter = converters
                .FirstOrDefault(c => c is DecisionContextConverter<AuditContext>);
            
            if (auditContextConverter.HasValue())
            {
                converters.Remove(auditContextConverter);
            }
            else
            {
                converters.Add(new DecisionContextConverter<AuditContext>());
            }
            // newOptions.Converters.Add(new DecisionContextConverter<DecisionContext>());
            // newOptions.Converters.Add(new DecisionContextConverter<CdsFinalisation>());
        }
        
        foreach (var jsonConverter in converters)
        {
            newOptions.Converters.Add(jsonConverter);
        }
        
        // 
        
        TType result = JsonSerializer.Deserialize<TType>(ref reader, newOptions)!;

        return result;   
    }

    public override void Write(Utf8JsonWriter writer, TType value, JsonSerializerOptions options)
    {
        if (value is DecisionContext)
        {
            JsonSerializer.Serialize(writer, value as DecisionContext, options);
        }
        else if (value is CdsFinalisation)
        {
            JsonSerializer.Serialize(writer, value as CdsFinalisation, options);
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}


// [BsonKnownTypes(typeof(DecisionContext), typeof(CdsFinalisation))]
// public interface IAuditContext
// {
//     
// }
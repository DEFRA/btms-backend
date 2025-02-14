using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Bson.Serialization.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Btms.Backend.OpenApi;

public class SchemaFilter : ISchemaFilter
{
    private readonly JsonNamingPolicy _namingPolicy = JsonNamingPolicy.CamelCase;

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (IsBsonIgnoreField(schema, context))
            return;

        foreach (var propertyInfo in context.Type.GetProperties())
        {
            if (HasBsonIgnoreAttribute(propertyInfo))
                schema.Properties.Remove(GetPropertyName(propertyInfo));
        }

        schema.Enum = GetEnums(context);
    }

    private static IList<IOpenApiAny> GetEnums(SchemaFilterContext context)
    {
        var enumOpenApiStrings = new List<IOpenApiAny>();
        if (!context.Type.IsEnum) return enumOpenApiStrings;

        enumOpenApiStrings.AddRange((from object? enumValue in Enum.GetValues(context.Type) select new OpenApiString(enumValue.ToString())).Cast<IOpenApiAny>());

        return enumOpenApiStrings;
    }

    private string GetPropertyName(PropertyInfo propertyInfo)
    {
        var jsonAttr = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
        return _namingPolicy.ConvertName(jsonAttr != null ? jsonAttr.Name : propertyInfo.Name);
    }
    private static bool HasBsonIgnoreAttribute(PropertyInfo propertyInfo)
    {
        return propertyInfo.GetCustomAttribute(typeof(BsonIgnoreAttribute)) != null;
    }

    private static bool IsBsonIgnoreField(OpenApiSchema schema, SchemaFilterContext context)
    {
        return schema.Properties == null || context.Type == null;
    }


}
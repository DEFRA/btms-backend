using Json.Schema;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel;

public static class JsonSchemaExtensions
{
    public static bool IsClass(this JsonSchema jsonSchema)
    {
        var typeKeyword = jsonSchema.GetKeyword<TypeKeyword>();

        if (typeKeyword != null)
        {
            return typeKeyword.Type == SchemaValueType.Object;
        }

        return false;
    }

    public static bool IsClassAndHasProperties(this JsonSchema jsonSchema)
    {
        var typeKeyword = jsonSchema.GetKeyword<TypeKeyword>();
        var propertiesKeyword = jsonSchema.GetKeyword<PropertiesKeyword>();

        if (typeKeyword != null)
        {
            return typeKeyword.Type == SchemaValueType.Object && propertiesKeyword != null;
        }

        return false;
    }

    public static string GetDescription(this JsonSchema jsonSchema)
    {
        var keyword = jsonSchema.GetKeyword<DescriptionKeyword>();

        return keyword != null ? keyword.Value : string.Empty;
    }

    public static bool IsReferenceType(this JsonSchema jsonSchema)
    {
        var typeKeyword = jsonSchema.GetKeyword<RefKeyword>();

        return typeKeyword != null;
    }

    public static bool IsEnum(this JsonSchema jsonSchema)
    {
        var typeKeyword = jsonSchema.GetKeyword<EnumKeyword>();

        return typeKeyword != null;
    }

    public static string ToCSharpType(this SchemaValueType schemaValueType, string propertyName)
    {
        return schemaValueType switch
        {
            SchemaValueType.Object => "IDictionary<string, string>",
            SchemaValueType.Array => "string[]",
            SchemaValueType.Boolean => "bool",
            SchemaValueType.String => "string",
            SchemaValueType.Number => "double",
            SchemaValueType.Integer => "int",
            SchemaValueType.Null => "null",
            _ => throw new ArgumentOutOfRangeException(nameof(schemaValueType), schemaValueType, null)
        };
    }

    public static string ToCSharpArrayType(this SchemaValueType schemaValueType)
    {
        return schemaValueType switch
        {
            SchemaValueType.Object => "object[]",
            SchemaValueType.Array => "string[]",
            SchemaValueType.Boolean => "bool[]",
            SchemaValueType.String => "string[]",
            SchemaValueType.Number => "double[]",
            SchemaValueType.Integer => "int[]",
            SchemaValueType.Null => "null",
            _ => throw new ArgumentOutOfRangeException(nameof(schemaValueType), schemaValueType, null)
        };
    }

    public static bool IsAllUpper(string input)
    {
        return input.All(char.IsUpper);
    }
}
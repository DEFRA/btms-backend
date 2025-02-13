using System.Diagnostics;
using Btms.Common.Extensions;
using Humanizer;

namespace Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

[DebuggerDisplay("{SourceName}")]
public class PropertyDescriptor
{
    private readonly bool _isReferenceType;
    private readonly bool _isArray;

    // private readonly string _classNamePrefix;
    private bool _typeOverridden;

    public PropertyDescriptor(string schemaName, string type,
        bool isReferenceType, bool isArray)
    {
        SchemaName = schemaName;
        SourceName = schemaName;
        InternalName = schemaName;

        _isReferenceType = isReferenceType;
        _isArray = isArray;
        Type = type;
        IsReferenceType = isReferenceType;
        IsArray = isArray;

        if (type.EndsWith("Enum"))
        {
            InternalAttributes.Add(
                "[MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]");
        }
    }

    public bool NoAttributes { get; set; } = default;

    /// <summary>
    /// The name in the schema the code is being generated from
    /// </summary>
    public string SchemaName { get; set; }

    /// <summary>
    /// The name we want in the Source Type Library
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// The value we want for the JsonProperty attribute in the Source Type Library
    /// </summary>
    public string? SourceJsonPropertyName { get; set; }

    /// <summary>
    /// The name we want in the Internal Data Model
    /// </summary>
    public string InternalName { get; set; }

    /// <summary>
    /// The value we want for the JsonProperty attribute in the Internal data model
    /// </summary>
    public string? InternalJsonPropertyName { get; set; }

    /// <summary>
    /// The Type to use (in the Source Type Library??)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The Type to use in the Internal Data Model
    /// </summary>
    public string? InternalType { get; set; }

    private readonly string? _description;

    public string? Description
    {
        get => _description;
        init => _description = value?.Replace("\n", " ");
    }

    /// <summary>
    /// Allows attributes to be added to the property in the Source Type Library
    /// </summary>
    public List<string> SourceAttributes { get; set; } = [];

    /// <summary>
    /// Allows attributes to be added to the property in the Internal data model
    /// </summary>
    public List<string> InternalAttributes { get; set; } = [];

    public bool IsReferenceType { get; set; }

    public bool IsNullable { get; set; }

    public bool IsArray { get; set; }

    public string Mapper { get; set; } = null!;

    public bool MappingInline { get; set; }

    public bool ExcludedFromInternal { get; set; }

    public bool ExcludedFromSource { get; set; }

    public void OverrideType(string type, string internalType)
    {
        Type = type;
        InternalType = internalType;
        _typeOverridden = true;
    }

    public string GetSourcePropertyName()
    {
        var n = SourceName.Dehumanize();
        if (SourceName.Equals("type", StringComparison.InvariantCultureIgnoreCase) ||
            SourceName.Equals("id", StringComparison.InvariantCultureIgnoreCase))
        {
            // if (SourceName.StartsWith(_classNamePrefix))
            // {
            //     return $"{SourceName.Dehumanize()}";
            // }

            return SourceName.Dehumanize();
        }

        if (_isArray)
        {
            n = n.Pluralize();
        }

        if (n.Contains("ID", StringComparison.CurrentCulture))
        {
            n = n.Replace("ID", "Id");
        }

        return n;
    }

    public string GetInternalPropertyName()
    {
        var n = InternalName.Dehumanize();
        if (InternalName.Equals("type", StringComparison.InvariantCultureIgnoreCase) ||
            InternalName.Equals("id", StringComparison.InvariantCultureIgnoreCase))
        {
            // if (InternalName.StartsWith(_classNamePrefix))
            // {
            //     return $"{InternalName.Dehumanize()}";
            // }

            return InternalName.Dehumanize();
        }

        if (_isArray)
        {
            n = n.Pluralize();
        }

        if (n.Contains("ID", StringComparison.CurrentCulture))
        {
            n = n.Replace("ID", "Id");
        }

        return n;
    }

    private string GetInternalJsonPropertyName()
    {
        return GetInternalPropertyName().FirstCharToLower();
    }

    public string[] GetSourceAttributes()
    {
        if (NoAttributes) return [];

        var defaultParams = new List<string>() { $"[JsonPropertyName(\"{SourceJsonPropertyName ?? SchemaName}\")]" };
        defaultParams.AddRange(SourceAttributes);

        return defaultParams.ToArray();
    }

    public string[] GetInternalAttributes()
    {
        if (NoAttributes) return [];

        var defaultParams = new List<string>() { "[Attr]", $"[JsonPropertyName(\"{InternalJsonPropertyName ?? GetInternalJsonPropertyName()}\")]" };

        if (!string.IsNullOrEmpty(Description))
        {
            defaultParams.Add($"[System.ComponentModel.Description(\"{Description}\")]");
        }

        defaultParams.AddRange(InternalAttributes);

        return defaultParams.ToArray();
    }

    public string GetInternalPropertyType()
    {
        return GetPropertyType(InternalType ?? Type);
    }

    public string GetSourcePropertyType()
    {
        return GetPropertyType(Type);
    }

    private string GetPropertyType(string t)
    {
        if (_typeOverridden)
        {
            return t;
        }

        if (_isReferenceType && !Type.Equals("Result") && !Type.Equals("Unit") && !Type.Equals("string") &&
            !Type.Equals("InspectionRequired"))
        {
            t = ClassDescriptor.BuildClassName(Type);
        }

        if (IsArray && !t.Contains("[]"))
        {
            t = $"{t}[]";
        }

        return t;
    }

    public string GetSourcePropertyTypeName()
    {
        return GetSourcePropertyType().Replace("[]", "");
    }

    public string GetInternalPropertyTypeName()
    {
        return GetInternalPropertyType().Replace("[]", "");
    }
}
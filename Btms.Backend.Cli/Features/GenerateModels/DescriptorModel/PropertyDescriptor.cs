using System.Diagnostics;
using Btms.Common.Extensions;
using Humanizer;

namespace Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

[DebuggerDisplay("{SourceName}")]
public class PropertyDescriptor
{
    private readonly bool _isReferenceType;

    private readonly bool _isArray;

    private readonly string _classNamePrefix;
    private bool _typeOverridden;

    public PropertyDescriptor(string sourceName, string type, string description, bool isReferenceType,
        bool isArray, string classNamePrefix)
        : this(sourceName, sourceName, type, description, isReferenceType, isArray, classNamePrefix)
    {
    }

    public PropertyDescriptor(string sourceName, string internalName, string type, string? description,
        bool isReferenceType, bool isArray, string classNamePrefix)
    {
        SourceName = sourceName;
        InternalName = internalName;

        _isReferenceType = isReferenceType;
        _isArray = isArray;
        _classNamePrefix = classNamePrefix;

        Type = type;

        Description = description?.Replace("\n", " ");
        IsReferenceType = isReferenceType;
        IsArray = isArray;
        // SourceAttributes = []; // [$"[JsonPropertyName(\"{sourceName}\")]"];
        // InternalAttributes = ["[Attr]", $"[System.ComponentModel.Description(\"{Description}\")]", $"[JsonPropertyName(\"{sourceName}\")]"];

        if (type.EndsWith("Enum"))
        {
            InternalAttributes.Add(
                "[MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]");
        }
    }

    public bool NoAttributes { get; set; } = default;

    public string SourceName { get; set; }

    public string? SourceJsonPropertyName { get; set; }

    public string? InternalJsonPropertyName { get; set; }

    public string InternalName { get; set; }

    public string Type { get; set; }

    public string? InternalType { get; set; }

    public string? Description { get; set; }

    public List<string> SourceAttributes { get; set; } = [];

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
            if (SourceName.StartsWith(_classNamePrefix))
            {
                return $"{SourceName.Dehumanize()}";
            }

            return $"{_classNamePrefix}{SourceName.Dehumanize()}";
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
            if (InternalName.StartsWith(_classNamePrefix))
            {
                return $"{InternalName.Dehumanize()}";
            }

            return $"{_classNamePrefix}{InternalName.Dehumanize()}";
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

    public string[] GetSourceAttributes()
    {
        if (NoAttributes) return [];

        var defaultParams = new List<string>() { $"[JsonPropertyName(\"{SourceJsonPropertyName ?? SourceName}\")]" };
        defaultParams.AddRange(SourceAttributes);

        return defaultParams.ToArray();
    }

    public string[] GetInternalAttributes()
    {
        if (NoAttributes) return [];

        var defaultParams = new List<string>() { "[Attr]", $"[JsonPropertyName(\"{InternalJsonPropertyName ?? InternalName}\")]" };

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
            t = ClassDescriptor.BuildClassName(Type, _classNamePrefix);
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
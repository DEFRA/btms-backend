using System.Diagnostics;
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

    public PropertyDescriptor(string sourceName, string internalName, string type, string description,
        bool isReferenceType, bool isArray, string classNamePrefix)
    {
        SourceName = sourceName;
        InternalName = internalName;

        _isReferenceType = isReferenceType;
        _isArray = isArray;
        _classNamePrefix = classNamePrefix;
        Type = type;
        Description = description;
        IsReferenceType = isReferenceType;
        IsArray = isArray;
        SourceAttributes = [$"[JsonPropertyName(\"{sourceName}\")]"];
        InternalAttributes = ["[Attr]", $"[System.ComponentModel.Description(\"{Description}\")]"];

        if (type.EndsWith("Enum"))
        {
            InternalAttributes.Add(
                "[MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.String)]");
        }
    }
    public string SourceName { get; set; }

    public string InternalName { get; set; }

    public string Type { get; set; }

    public string Description { get; set; }

    public List<string> SourceAttributes { get; set; }

    public List<string> InternalAttributes { get; set; }

    public bool IsReferenceType { get; set; }

    public bool IsNullable { get; set; }

    public bool IsArray { get; set; }

    public string Mapper { get; set; } = null!;


    public bool MappingInline { get; set; }

    public bool ExcludedFromInternal { get; set; } = false;

    public bool ExcludedFromSource { get; set; } = false;

    public void OverrideType(string type)
    {
        Type = type;
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

    public string GetPropertyType()
    {
        var t = Type;

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

    public string GetPropertyTypeName()
    {
        return GetPropertyType().Replace("[]", "");
    }
}
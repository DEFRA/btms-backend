using Btms.Common.Extensions;

namespace Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

public enum Model
{
    Source,
    Internal,
    Both
}

public enum DatetimeType
{
    Epoch,
    Local,
    Utc
}

internal class PropertyMap(string name)
{
    public string Name { get; set; } = name;

    public string Type { get; set; } = null!;

    public string InternalType { get; set; } = null!;

    public bool InternalTypeOverwritten { get; set; }

    public bool TypeOverwritten { get; set; }

    public List<string> SourceAttributes { get; set; } = new();

    public List<string> InternalAttributes { get; set; } = new();

    public bool AttributesOverwritten { get; set; }

    public string OverriddenSourceName { get; set; } = null!;

    public string OverriddenInternalName { get; set; } = null!;

    public bool SourceNameOverwritten { get; set; }

    public bool InternalNameOverwritten { get; set; }

    public bool NoAttributes { get; set; }

    public bool ExcludedFromInternal { get; set; }

    public bool ExcludedFromSource { get; set; }

    public MapperMap? Mapper { get; set; }

    public class MapperMap
    {
        public bool Inline { get; set; }

        public string Name { get; set; } = null!;
    }

    public PropertyMap SetInternalType(string type)
    {
        InternalType = type ?? throw new ArgumentNullException("type");
        InternalTypeOverwritten = true;
        return this;
    }

    public PropertyMap SetType(string type, string? internalType = null)
    {
        Type = type ?? throw new ArgumentNullException("type");
        InternalType = internalType ?? type;
        InternalTypeOverwritten = internalType.HasValue();
        TypeOverwritten = true;
        return this;
    }

    public PropertyMap IsDateTime(DatetimeType? type = null)
    {
        SetType("DateTime");
        if (type == DatetimeType.Epoch)
        {
            AddAttribute("[JsonConverter(typeof(DateTimeConverterUsingDateTimeParse))]", Model.Source);
        }
        return this;
    }

    public PropertyMap IsDate()
    {
        SetType("DateOnly");
        return this;
    }

    public PropertyMap IsTime()
    {
        SetType("TimeOnly");
        return this;
    }

    public PropertyMap SetName(string name, string? internalName = null)
    {
        SetSourceName(name);
        SetInternalName(internalName ?? name);
        return this;
    }

    public PropertyMap SetSourceName(string name)
    {
        OverriddenSourceName = name ?? throw new ArgumentNullException("name");
        SourceNameOverwritten = true;
        return this;
    }

    public PropertyMap SetInternalName(string name)
    {
        OverriddenInternalName = name ?? throw new ArgumentNullException("name");
        InternalNameOverwritten = true;
        return this;
    }

    public PropertyMap IsSensitive()
    {
        AddAttribute("[Btms.SensitiveData.SensitiveData]", Model.Source);
        return this;
    }

    public PropertyMap SetBsonIgnore()
    {
        AddAttribute("[MongoDB.Bson.Serialization.Attributes.BsonIgnore]", Model.Internal);
        return this;
    }

    public PropertyMap ExcludeFromInternal()
    {
        ExcludedFromInternal = true;
        return this;
    }

    public PropertyMap ExcludeFromSource()
    {
        ExcludedFromSource = true;
        return this;
    }

    public PropertyMap SetMapper(string mapperName, bool inline = false)
    {
        Mapper = new MapperMap { Inline = inline, Name = mapperName };
        return this;
    }

    public PropertyMap AddAttribute(string attribute, Model model)
    {
        if (string.IsNullOrEmpty(attribute))
        {
            throw new ArgumentNullException("attribute");
        }

        switch (model)
        {
            case Model.Source:
                SourceAttributes.Add(attribute);
                break;
            case Model.Internal:
                InternalAttributes.Add(attribute);
                break;
            case Model.Both:
                SourceAttributes.Add(attribute);
                InternalAttributes.Add(attribute);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(model), model, null);
        }

        AttributesOverwritten = true;
        return this;
    }

    public PropertyMap NoAttribute(Model model)
    {
        switch (model)
        {
            case Model.Source:
                SourceAttributes.Clear();
                break;
            case Model.Internal:
                InternalAttributes.Clear();
                break;
            case Model.Both:
                SourceAttributes.Clear();
                InternalAttributes.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(model), model, null);
        }

        AttributesOverwritten = true;
        NoAttributes = true;
        return this;
    }
}
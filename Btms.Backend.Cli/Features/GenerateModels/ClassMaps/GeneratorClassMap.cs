using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

namespace Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

internal class GeneratorClassMap
{
    private static readonly Dictionary<string, GeneratorClassMap> ClassMaps = new();

    public GeneratorClassMap(string className, Action<GeneratorClassMap> classMapInitializer)
    {
        Name = className;
        SourceClassName = className;
        InternalClassName = className;
        classMapInitializer(this);
    }

    public string Name { get; set; }

    public string SourceClassName { get; private set; }

    public string InternalClassName { get; private set; }

    public bool ExcludedFromInternal { get; private set; }

    public List<PropertyMap> Properties { get; private set; } = [];

    public List<PropertyDescriptor> NewProperties { get; private set; } = [];

    public GeneratorClassMap SetClassName(string className, string? internalClassName = null)
    {
        SetSourceClassName(className);
        SetInternalClassName(internalClassName ?? className);
        return this;
    }

    public GeneratorClassMap ExcludeFromInternal()
    {
        ExcludedFromInternal = true;
        return this;
    }

    public GeneratorClassMap SetSourceClassName(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            throw new ArgumentNullException(nameof(className));
        }

        SourceClassName = className;
        return this;
    }

    public GeneratorClassMap SetInternalClassName(string className)
    {
        if (string.IsNullOrEmpty(className))
        {
            throw new ArgumentNullException(nameof(className));
        }

        InternalClassName = className;
        return this;
    }

    public void AddProperty(PropertyDescriptor property)
    {
        if (property == null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        NewProperties.Add(property);
    }

    public void MapDateOnlyAndTimeOnlyToDateTimeProperty(string dateOnlyProperty, string timeOnlyProperty,
        string dateTimeProperty)
    {
        MapProperty(timeOnlyProperty).IsTime().ExcludeFromInternal();
        MapProperty(dateOnlyProperty).IsDate().ExcludeFromInternal();
        AddProperty(new PropertyDescriptor(dateTimeProperty, "DateTime", false, false)
        {
            Description = "DateTime",
            ExcludedFromSource = true,
            Mapper =
                $"DateTimeMapper.Map(from?.{PascalCaseNamingPolicy.ConvertName(dateOnlyProperty)}, from?.{PascalCaseNamingPolicy.ConvertName(timeOnlyProperty)});",
            MappingInline = true,
        });
    }

    public PropertyMap MapProperty(string propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        var propertyMap = new PropertyMap(propertyName);
        Properties.Add(propertyMap);
        return propertyMap;
    }

    public static void Reset()
    {
        ClassMaps.Clear();
    }

    public static GeneratorClassMap RegisterClassMap(string name, Action<GeneratorClassMap> classMapInitializer)
    {
        var classMap = new GeneratorClassMap(name, classMapInitializer);
        ClassMaps.Add(classMap.Name.ToLower(), classMap);
        return classMap;
    }

    public static GeneratorClassMap? LookupClassMap(string name)
    {
        ClassMaps.TryGetValue(name.ToLower(), out var classMap);
        return classMap;
    }
}
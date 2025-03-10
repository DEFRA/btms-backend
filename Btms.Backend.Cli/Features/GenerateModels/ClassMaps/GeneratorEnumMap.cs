using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

namespace Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

internal class GeneratorEnumMap
{
    private static readonly Dictionary<string, GeneratorEnumMap> ClassMaps = new();


    public GeneratorEnumMap(string className, Action<GeneratorEnumMap> classMapInitializer)
    {
        Name = className;
        classMapInitializer(this);
    }

    public string Name { get; set; }

    public List<EnumDescriptor.EnumValueDescriptor> EnumValues = new();
    public List<string> EnumValuesToRemove = new();
    public List<(string OldValue, string NewValue)> EnumValuesToRename = new();

    public GeneratorEnumMap AddEnumValue(string value)
    {
        EnumValues.Add(new EnumDescriptor.EnumValueDescriptor(value));
        return this;
    }

    public GeneratorEnumMap RemoveEnumValue(string value)
    {
        EnumValuesToRemove.Add(value);
        return this;
    }

    public GeneratorEnumMap RenameEnumValue(string oldValue, string newValue)
    {
        EnumValuesToRename.Add(new ValueTuple<string, string>(oldValue, newValue));
        return this;
    }

    public static GeneratorEnumMap RegisterEnumMap(string name, Action<GeneratorEnumMap> classMapInitializer)
    {
        var classMap = new GeneratorEnumMap(name, classMapInitializer);
        ClassMaps.Add(classMap.Name, classMap);
        return classMap;
    }

    public static GeneratorEnumMap? LookupEnumMap(string name)
    {
        ClassMaps.TryGetValue(name, out var classMap);
        return classMap;
    }

    public static void Reset()
    {
        ClassMaps.Clear();
    }
}
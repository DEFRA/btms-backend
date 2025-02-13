using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;

namespace Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

public class OutputFile
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required string Content { get; set; }
}

public class CSharpDescriptor
{
    public List<ClassDescriptor> Classes { get; set; } = [];

    public List<EnumDescriptor> Enums { get; set; } = [];

    public List<OutputFile> OutputFiles { get; set; } = [];

    public List<string> FilesToEnsureDontExist { get; set; } = [];

    // There are static properties on ClassDescriptor & EnumDescriptor
    // that need resetting if we want to use CSharpDescriptor
    // multiple times (e.g in unit tests).
    // We might want to move that state into CSharpDescriptor instead
    public CSharpDescriptor()
    {
        // GeneratorClassMap.Reset();
        // GeneratorEnumMap.Reset();
    }

    public void AddEnumDescriptor(EnumDescriptor enumDescriptor)
    {
        if (Enums.All(x => x.GetEnumName() != enumDescriptor.GetEnumName()))
        {
            Enums.Add(enumDescriptor);
        }
    }

    public void AddClassDescriptor(ClassDescriptor classDescriptor)
    {
        if (Classes.All(x => x.Name != classDescriptor.Name))
        {
            Classes.Add(classDescriptor);
        }
        else
        {
            var existing = Classes.First(x => x.Name == classDescriptor.Name);
            foreach (var propertyDescriptor in classDescriptor.Properties)
            {
                existing.AddPropertyDescriptor(propertyDescriptor);
            }
        }
    }
}
using System.Diagnostics;
using Humanizer;

namespace Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

[DebuggerDisplay("{Name}")]
public class ClassDescriptor(string name, string sourceNamespace, string internalNamespace, string? internalName = null)
{
    public string Name { get; set; } = name;

    public string InternalName { get; set; } = internalName ?? name;

    public bool IgnoreInternalClass { get; set; }
    public string SourceNamespace { get; } = sourceNamespace;

    public string InternalNamespace { get; } = internalNamespace;

    public string Description { get; set; } = null!;

    public bool IsResource { get; set; }

    public List<PropertyDescriptor> Properties { get; set; } = [];

    public void AddPropertyDescriptor(PropertyDescriptor propertyDescriptor)
    {
        if (Properties.TrueForAll(x => x.SourceName != propertyDescriptor.SourceName))
        {
            Properties.Add(propertyDescriptor);
        }
    }

    public string GetClassName()
    {
        return BuildClassName(Name);
    }

    public string GetInternalClassName()
    {
        return BuildClassName(InternalName);
    }

    public string GetSourceFullClassName()
    {
        return $"{SourceNamespace}.{BuildClassName(Name)}";
    }

    public string GetInternalFullClassName()
    {
        return $"{InternalNamespace}.{BuildClassName(InternalName)}";
    }

    public static string BuildClassName(string name)
    {

        return name.Dehumanize();
    }
}
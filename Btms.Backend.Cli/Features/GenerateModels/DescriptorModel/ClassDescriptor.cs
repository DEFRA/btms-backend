using System.Diagnostics;
using Humanizer;

namespace Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

[DebuggerDisplay("{Name}")]
public class ClassDescriptor(string name, string sourceNamespace, string internalNamespace, string classNamePrefix, string? internalName = null)
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
        return BuildClassName(Name, classNamePrefix, IsResource);
    }

    public string GetInternalClassName()
    {
        return BuildClassName(InternalName, classNamePrefix, IsResource);
    }

    public string GetSourceFullClassName()
    {
        return $"{SourceNamespace}.{BuildClassName(Name, classNamePrefix, IsResource)}";
    }

    public string GetInternalFullClassName()
    {
        return $"{InternalNamespace}.{BuildClassName(InternalName, classNamePrefix, IsResource)}";
    }

    public static string BuildClassName(string name, string? classNamePrefix, bool isResource = false)
    {
        if (classNamePrefix != null && name.StartsWith(classNamePrefix))
        {
            return name.Dehumanize();
        }

        return isResource ? name.Dehumanize() : $"{classNamePrefix}{name.Dehumanize()}";
    }
}
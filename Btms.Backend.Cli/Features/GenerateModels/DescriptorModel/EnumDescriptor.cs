using System.Diagnostics;
using Humanizer;

namespace Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;

[DebuggerDisplay("{Name}")]
public class EnumDescriptor(string name, string? parentName, string sourceNamespace, string internalNamespace)
{
    private const string Suffix = "Enum";

    public string Name { get; set; } = name;

    public string FullName { get; set; } = BuildEnumName(name, parentName);

    public string SourceNamespace { get; } = sourceNamespace;

    public string InternalNamespace { get; } = internalNamespace;

    public List<EnumValueDescriptor> Values { get; set; } = [];

    public void AddValues(List<EnumValueDescriptor> values)
    {
        Values.AddRange(values);
    }

    public string GetEnumName()
    {
        return BuildEnumName(Name, parentName);
    }

    public string GetSourceFullEnumName()
    {
        return $"{SourceNamespace}.{BuildEnumName(Name, parentName)}";
    }

    public string GetInternalFullEnumName()
    {
        return $"{InternalNamespace}.{BuildEnumName(Name, parentName)}";
    }

    public class EnumValueDescriptor(string value)
    {
        public string Value { get; set; } = value;

        public string OverriddenValue { get; set; } = null!;

        public string GetCSharpValue()
        {
            if (!string.IsNullOrEmpty(OverriddenValue))
            {
                return OverriddenValue;
            }

            if (Value.Contains("_"))
            {
                return Value.Replace("_", "-").ToLower().Dehumanize();
            }
            else
            {
                if (Value.Contains(','))
                {
                    try
                    {
                        return Value.Replace(",", "").Dehumanize();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                }

                var v = Value.Dehumanize();
                if (v.All(char.IsUpper))
                {
                    return v.ToLower().Pascalize();
                }

                return v;
            }
        }
    }

    public static string BuildEnumName(string name, string? parentName)
    {
        if (string.IsNullOrEmpty(parentName))
        {
            return $"{name.Dehumanize()}{Suffix}";
        }

        return $"{parentName}{name.Dehumanize()}{Suffix}";
    }
}
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using Btms.Common.Extensions;
using RazorLight;

namespace Btms.Backend.Cli.Features.GenerateModels;

internal class CSharpFileBuilder
{
    public static async Task Build(CSharpDescriptor descriptor, string sourceOutputPath, string internalOutputPath,
        string mappingOutputPath, CancellationToken cancellationToken = default)
    {
        await Generate(descriptor, sourceOutputPath, internalOutputPath, mappingOutputPath);
        await Save(descriptor, cancellationToken);
    }

    private static async Task Save(CSharpDescriptor descriptor, CancellationToken cancellationToken = default)
    {
        await descriptor.OutputFiles.ForEachAsync(async f =>
            await File.WriteAllTextAsync(f.Path, f.Content, cancellationToken));

        descriptor.FilesToEnsureDontExist.ForEach(File.Delete);
    }

    internal static async Task Generate(CSharpDescriptor descriptor, string sourceOutputPath, string internalOutputPath,
        string mappingOutputPath)
    {
        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(System.Reflection.Assembly.GetExecutingAssembly(),
                "Btms.Backend.Cli.Features.GenerateModels.Templates")
            .UseMemoryCachingProvider()
            .Build();

        foreach (var @class in descriptor.Classes.OrderBy(x => x.Name))
        {
            ApplySourceClassMapOverrides(@class);

            //create source 

            var contents = await engine.CompileRenderAsync("ClassTemplate", @class);
            descriptor.OutputFiles.Add(new OutputFile()
            {
                Name = @class.GetClassName(),
                Path = Path.Combine(sourceOutputPath, $"{@class.GetClassName()}.g.cs"),
                Content = contents
            });

            Console.WriteLine($"Generated file: {@class.GetClassName()}.cs");

            //create internal 
            var internalPath = Path.Combine(internalOutputPath, $"{@class.GetInternalClassName()}.g.cs");
            var mapperPath = Path.Combine(mappingOutputPath, $"{@class.GetClassName()}Mapper.g.cs");

            if (@class.IgnoreInternalClass)
            {
                Console.WriteLine("{0} internal classes shouldn't be present. Deleting internal & mapper files if they exist.", @class.Name);
                descriptor.FilesToEnsureDontExist.Add(internalPath);
                descriptor.FilesToEnsureDontExist.Add(mapperPath);
            }
            else
            {
                contents = await engine.CompileRenderAsync("InternalClassTemplate", @class);

                descriptor.OutputFiles.Add(new OutputFile()
                {
                    Name = @class.GetInternalClassName(),
                    Path = internalPath,
                    Content = contents
                });

                Console.WriteLine($"Generated file: {@class.GetInternalClassName()}.cs");

                contents = await engine.CompileRenderAsync("MapperTemplate", @class);

                descriptor.OutputFiles.Add(new OutputFile()
                {
                    Name = $"{@class.GetClassName()}Mapper",
                    Path = mapperPath,
                    Content = contents
                });

                Console.WriteLine($"Generated file: {@class.GetClassName()}.cs");
            }
        }

        foreach (var @enum in descriptor.Enums.OrderBy(x => x.Name))
        {
            ApplyEnumMapOverrides(@enum);

            var contents = await engine.CompileRenderAsync("EnumTemplate", @enum);

            descriptor.OutputFiles.Add(new OutputFile()
            {
                Name = @enum.GetEnumName(),
                Path = Path.Combine(sourceOutputPath, $"{@enum.GetEnumName()}.g.cs"),
                Content = contents
            });

            Console.WriteLine($"Generated file: {@enum.GetEnumName()}.cs");

            contents = await engine.CompileRenderAsync("InternalEnumTemplate", @enum);

            descriptor.OutputFiles.Add(new OutputFile()
            {
                Name = @enum.GetEnumName(),
                Path = Path.Combine(internalOutputPath, $"{@enum.GetEnumName()}.g.cs"),
                Content = contents
            });

            Console.WriteLine($"Generated file: {@enum.GetEnumName()}.cs");

            contents = await engine.CompileRenderAsync("EnumMapperTemplate", @enum);

            descriptor.OutputFiles.Add(new OutputFile()
            {
                Name = $"{@enum.GetEnumName()}Mapper",
                Path = Path.Combine(mappingOutputPath, $"{@enum.GetEnumName()}Mapper.g.cs"),
                Content = contents
            });

            Console.WriteLine($"Generated file: {@enum.GetEnumName()}.cs");
        }
    }

    private static void ApplySourceClassMapOverrides(ClassDescriptor @class)
    {
        var classMap = GeneratorClassMap.LookupClassMap(@class.Name);

        if (classMap is not null)
        {
            @class.Name = classMap.SourceClassName;
            @class.InternalName = classMap.InternalClassName;
            @class.IgnoreInternalClass = classMap.ExcludedFromInternal;

            foreach (var propertyMap in classMap.Properties)
            {
                var propertyDescriptor = @class.Properties.FirstOrDefault(x =>
                    x.SourceName.Equals(propertyMap.Name, StringComparison.InvariantCultureIgnoreCase));

                if (propertyDescriptor is not null)
                {
                    if (propertyMap.TypeOverwritten)
                    {
                        propertyDescriptor.OverrideType(propertyMap.Type, propertyMap.InternalType);
                    }

                    if (propertyMap.SourceNameOverwritten)
                    {
                        propertyDescriptor.SourceName = propertyMap.OverriddenSourceName;
                    }

                    if (propertyMap.InternalNameOverwritten)
                    {
                        propertyDescriptor.InternalName = propertyMap.OverriddenInternalName;
                    }

                    if (propertyMap.AttributesOverwritten)
                    {
                        if (propertyMap.NoAttributes)
                        {
                            propertyDescriptor.NoAttributes = true;
                            propertyDescriptor.InternalAttributes.Clear();
                        }
                        else
                        {
                            propertyDescriptor.SourceAttributes = propertyMap.SourceAttributes;
                            propertyDescriptor.InternalAttributes.AddRange(propertyMap.InternalAttributes);
                        }
                    }

                    propertyDescriptor.SourceJsonPropertyName = propertyMap.SourceJsonPropertyName;
                    propertyDescriptor.InternalJsonPropertyName = propertyMap.InternalJsonPropertyName;
                    propertyDescriptor.ExcludedFromSource = propertyMap.ExcludedFromSource;
                    propertyDescriptor.ExcludedFromInternal = propertyMap.ExcludedFromInternal;
                    propertyDescriptor.DateTimeType = propertyMap.DateTimeType;
                    propertyDescriptor.DateOnlyType = propertyMap.DateOnlyType;

                    if (propertyMap.Mapper is not null)
                    {
                        propertyDescriptor.Mapper = propertyMap.Mapper.Name;
                        propertyDescriptor.MappingInline = propertyMap.Mapper.Inline;
                    }
                }
            }

            foreach (var propertyMap in classMap.NewProperties)
            {
                @class.AddPropertyDescriptor(propertyMap);
            }
        }
    }

    private static void ApplyEnumMapOverrides(EnumDescriptor @enum)
    {
        var classMap = GeneratorEnumMap.LookupEnumMap(@enum.FullName);

        if (classMap is not null)
        {
            foreach (var v in classMap.EnumValuesToRemove)
            {
                @enum.Values.RemoveAll(x => x.Value == v);
            }

            foreach (var v in classMap.EnumValuesToRename)
            {
                var item = @enum.Values.SingleOrDefault(x => x.Value == v.OldValue);
                if (item is not null)
                {
                    item.OverriddenValue = v.NewValue;
                }
            }

            @enum.Values.AddRange(classMap.EnumValues);
        }
    }
}
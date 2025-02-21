using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using Humanizer;
using Json.Schema;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;

public class DescriptorBuilderSchemaVisitor : ISchemaVisitor
{
    public void OnProperty(PropertyVisitorContext context)
    {
        var typeKeyword = context.JsonSchema.GetKeyword<TypeKeyword>();
        var description = context.JsonSchema.GetDescription();

        if (typeKeyword is not null)
        {
            if (typeKeyword.Type == SchemaValueType.Array)
            {
                var itemsKeyword = context.JsonSchema.GetKeyword<ItemsKeyword>();
                if (itemsKeyword is not null)
                {
                    if (itemsKeyword.SingleSchema!.IsReferenceType())
                    {
                        var refKeyword = itemsKeyword.SingleSchema?.GetKeyword<RefKeyword>();
#pragma warning disable S6608
                        var value = refKeyword?.Reference.ToString().Split('/').Last();
#pragma warning restore S6608

                        context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key,
                            $"{value}",
                            true, true)
                        {
                            Description = description
                        });
                    }
                    else
                    {
                        var itemType = itemsKeyword.SingleSchema!.GetKeyword<TypeKeyword>()!.Type;
                        if (itemType != SchemaValueType.Object)
                        {
                            context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key,
                                itemType.ToCSharpArrayType(),
                                false, true)
                            {
                                Description = description
                            });
                        }
                        else
                        {
                            OnDefinition(new DefinitionVisitorContext(context.CSharpDescriptor, context.RootJsonSchema, context.Key, itemsKeyword.SingleSchema));
                            context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key,
                                context.Key,
                                true, true)
                            {
                                Description = description
                            });
                        }
                    }

                }
            }
            else
            {
                if (context.JsonSchema.IsClassAndHasProperties())
                {
                    var propertiesKeyword = context.JsonSchema.GetKeyword<PropertiesKeyword>();
                    var classDescriptor = new ClassDescriptor(context.Key.Dehumanize(), IpaffsDescriptorBuilder.SourceNamespace, IpaffsDescriptorBuilder.InternalNamespace);
                    classDescriptor.Description = context.JsonSchema.GetDescription();


                    foreach (var property in propertiesKeyword?.Properties!)
                    {
                        OnProperty(new PropertyVisitorContext(context.CSharpDescriptor, classDescriptor, context.RootJsonSchema, property.Key,
                            property.Value));
                    }
                    context.CSharpDescriptor.AddClassDescriptor(classDescriptor);
                    context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key, context.Key,
                        true, false)
                    {
                        Description = description
                    });
                }
                else
                {
                    var t = typeKeyword.Type.ToCSharpType(context.Key);
                    var referenceType = false;
                    if (context.JsonSchema.IsEnum())
                    {
                        t = EnumDescriptor.BuildEnumName(context.Key, context.ClassDescriptor.Name);
                        referenceType = true;
                    }

                    context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key, t,
                        referenceType, false)
                    {
                        Description = description
                    });
                }

            }

        }
        else
        {
            var refKeyword = context.JsonSchema.GetKeyword<RefKeyword>();
            if (refKeyword is not null)
            {
#pragma warning disable S6608
                var value = refKeyword.Reference.ToString().Split('/').Last();
#pragma warning restore S6608
                var definition = context.RootJsonSchema.GetDefinitions()!.FirstOrDefault(x =>
                    x.Key.Equals(value, StringComparison.InvariantCultureIgnoreCase));

                var defType = definition.Value.GetKeyword<TypeKeyword>()!.Type;

                if (defType == SchemaValueType.Object)
                {
                    context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key, value,
                        true, false)
                    {
                        Description = description
                    });
                }
                else if (defType == SchemaValueType.Array)
                {
                    var itemsKeyword = definition.Value.GetKeyword<ItemsKeyword>();
                    if (itemsKeyword is not null)
                    {
                        var itemType = itemsKeyword.SingleSchema!.GetKeyword<TypeKeyword>()!.Type;
                        if (itemType != SchemaValueType.Object)
                        {
                            context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key,
                                itemType.ToCSharpArrayType(),
                                false, true)
                            {
                                Description = description
                            });
                        }
                    }
                }
                else
                {
                    context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key, defType.ToCSharpType(context.Key),
                        false, false)
                    {
                        Description = description
                    });
                }
            }
            else
            {
                var propertiesKeyword = context.JsonSchema.GetKeyword<PropertiesKeyword>();
                var classDescriptor = new ClassDescriptor(context.Key.Dehumanize(), IpaffsDescriptorBuilder.SourceNamespace, IpaffsDescriptorBuilder.InternalNamespace);
                classDescriptor.Description = context.JsonSchema.GetDescription();
                context.CSharpDescriptor.AddClassDescriptor(classDescriptor);

                foreach (var property in propertiesKeyword!.Properties)
                {
                    OnProperty(new PropertyVisitorContext(context.CSharpDescriptor, classDescriptor, context.RootJsonSchema, property.Key,
                        property.Value));
                }

                context.ClassDescriptor.Properties.Add(new PropertyDescriptor(context.Key, context.Key,
                    true, false));
            }
        }

        if (context.JsonSchema.IsEnum())
        {
            OnEnum(context.CSharpDescriptor, context.JsonSchema, context.ClassDescriptor, context.Key);
        }
    }

    public void OnDefinition(DefinitionVisitorContext context)
    {
        if (context.JsonSchema.IsEnum())
        {
            OnEnum(context.CSharpDescriptor, context.JsonSchema, null!, context.Key);

        }
        else if (context.JsonSchema.IsClass())
        {
            var classDescriptor = new ClassDescriptor(context.Key.Dehumanize(), IpaffsDescriptorBuilder.SourceNamespace, IpaffsDescriptorBuilder.InternalNamespace)
            {
                Description = context.JsonSchema.GetDescription()
            };

            context.CSharpDescriptor.AddClassDescriptor(classDescriptor);

            var propertiesKeyword = context.JsonSchema.GetKeyword<PropertiesKeyword>();

            foreach (var property in propertiesKeyword!.Properties)
            {
                OnProperty(new PropertyVisitorContext(context.CSharpDescriptor, classDescriptor, context.RootJsonSchema, property.Key,
                    property.Value));
            }
        }
    }

    private void OnEnum(CSharpDescriptor cSharpDescriptor, JsonSchema schema, ClassDescriptor classDescriptor, string name)
    {
        var enumKeyword = schema.GetKeyword<EnumKeyword>();

        if (enumKeyword is not null)
        {
            var values = enumKeyword.Values.Select(x => new EnumDescriptor.EnumValueDescriptor(x!.ToString()))
                .ToList();

            cSharpDescriptor.AddEnumDescriptor(new EnumDescriptor(name, classDescriptor?.Name, IpaffsDescriptorBuilder.SourceNamespace, IpaffsDescriptorBuilder.InternalNamespace) { Values = values });
        }
    }
}
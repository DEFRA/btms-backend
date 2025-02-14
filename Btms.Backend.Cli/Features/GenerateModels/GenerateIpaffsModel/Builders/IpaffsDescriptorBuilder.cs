using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using Json.Schema;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;

public class IpaffsDescriptorBuilder(List<ISchemaVisitor> visitors)
{
    public const string SourceNamespace = "Btms.Types.Ipaffs";
    public const string InternalNamespace = "Btms.Model.Ipaffs";

    public CSharpDescriptor Build(string jsonSchema)
    {
        var mySchema = JsonSchema.FromText(jsonSchema);

        var csharpDescriptor = new CSharpDescriptor();

        var mainClassDescriptor =
            new ClassDescriptor("ImportNotification", SourceNamespace, InternalNamespace) { IsResource = true };

        csharpDescriptor.AddClassDescriptor(mainClassDescriptor);
        foreach (var property in mySchema.GetProperties()!)
        {
            visitors.ForEach(x => x.OnProperty(new PropertyVisitorContext(csharpDescriptor, mainClassDescriptor,
                mySchema, property.Key, property.Value)));
        }

        foreach (var definition in mySchema.GetDefinitions()!)
        {
            visitors.ForEach(x =>
                x.OnDefinition(new DefinitionVisitorContext(csharpDescriptor, mySchema, definition.Key,
                    definition.Value)));
        }

        return csharpDescriptor;
    }
}
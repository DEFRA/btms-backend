using System.Xml;
using System.Xml.Schema;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using CommandLine;
using MediatR;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateAlvsModel.Commands;

[Verb("generate-alvs-model", isDefault: false, HelpText = "Generates Csharp ALVS classes from XSD Schema.")]
internal class GenerateAlvsModelCommand : IRequest
{
    public const string SourceNamespace = "Btms.Types.Alvs";
    public const string InternalNamespace = "Btms.Model.Alvs";
    public const string ClassNamePrefix = "";

    //[Option('o', "sourceOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string SourceOutputPath { get; set; } = "D:\\repos\\esynergy\\Btms-Backend\\Btms.Types.Alvs.V1\\";

    // [Option('i', "internalOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string InternalOutputPath { get; set; } = "D:\\repos\\esynergy\\Btms-Backend\\Btms.Model\\Alvs\\";

    public string MappingOutputPath { get; set; } = "D:\\repos\\esynergy\\Btms-Backend\\Btms.Types.Alvs.Mapping.V1\\";

    public class Handler : AsyncRequestHandler<GenerateAlvsModelCommand>
    {
        protected override async Task Handle(GenerateAlvsModelCommand request, CancellationToken cancellationToken)
        {
#pragma warning disable S1075
            var reader = new XmlTextReader("D:\\repos\\esynergy\\Btms-Backend\\Btms.Backend.Cli\\Features\\GenerateModels\\GenerateAlvsModel\\sendALVSClearanceRequest.xsd");
#pragma warning restore S1075
            var schema = XmlSchema.Read(reader, ValidationCallback!)!;

            var csharpDescriptor = new CSharpDescriptor();

            foreach (var schemaItem in schema.Items)
            {
                if (schemaItem is XmlSchemaComplexType complexType)
                {
                    BuildClass(csharpDescriptor, complexType);
                }
            }

            await CSharpFileBuilder.Build(csharpDescriptor, request.SourceOutputPath, request.InternalOutputPath, request.MappingOutputPath, cancellationToken);
        }

        private void BuildClass(CSharpDescriptor cSharpDescriptor, XmlSchemaComplexType complexType)
        {
            var name = complexType.Name;

            if (string.IsNullOrEmpty(name))
            {
                name = ((XmlSchemaElement)complexType.Parent!).Name;
            }

            Console.WriteLine($"Class Name: {name}");
            var classDescriptor = new ClassDescriptor(name!, SourceNamespace, InternalNamespace, ClassNamePrefix);

            classDescriptor.Description = complexType.GetDescription();
            cSharpDescriptor.AddClassDescriptor(classDescriptor);

            if (complexType.Particle is XmlSchemaSequence sequence)
            {
                foreach (var sequenceItem in sequence.Items)
                {
                    if (sequenceItem is XmlSchemaElement schemaSequence && schemaSequence.SchemaType is XmlSchemaComplexType ct)
                    {
                        BuildClass(cSharpDescriptor, ct);
                    }

                    var schemaElement = sequenceItem as XmlSchemaElement;
                    Console.WriteLine($"Property Name: {schemaElement?.Name} - Type: {schemaElement?.GetSchemaType()}");
                    var propertyName = System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(schemaElement?.Name!);
                    var propertyDescriptor = new PropertyDescriptor(
                        sourceName: propertyName,
                        type: schemaElement?.GetSchemaType()!,
                        description: "",
                        isReferenceType: IsReferenceType(schemaElement!.GetSchemaType()),
                        isArray: schemaElement?.MaxOccursString == "unbounded",
                        classNamePrefix: ClassNamePrefix);
                    classDescriptor.Properties.Add(propertyDescriptor);
                }

            }
        }

        private static bool IsReferenceType(string type)
        {
            var nonReferenceTypes = new[] { "string", "DateTime", "int", "decimal" };
            return !nonReferenceTypes.Contains(type);
        }

        private static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.Write("WARNING: ");
            else if (args.Severity == XmlSeverityType.Error)
                Console.Write("ERROR: ");

            Console.WriteLine(args.Message);
        }
    }
}
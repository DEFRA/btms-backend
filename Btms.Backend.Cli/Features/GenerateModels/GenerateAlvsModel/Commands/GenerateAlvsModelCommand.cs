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
    public const string InternalNamespace = "Btms.Model.Cds";
    public const string SolutionPath = "../../../../";

    [Option('s', "schema",
        HelpText = "The xsd file, which to use to generate the csharp classes.")]
    public string SchemaFile { get; set; } =
        $"{SolutionPath}/Btms.Backend.Cli/Features/GenerateModels/GenerateAlvsModel/sendALVSClearanceRequest.xsd";

    public string SourceOutputPath { get; set; } = $"{SolutionPath}/Btms.Types.Alvs.V1/";

    public string InternalOutputPath { get; set; } = $"{SolutionPath}/Btms.Model/Cds/";

    public string MappingOutputPath { get; set; } = $"{SolutionPath}/Btms.Types.Alvs.Mapping.V1/";

    public class Handler : IRequestHandler<GenerateAlvsModelCommand>
    {
        public async Task Handle(GenerateAlvsModelCommand request, CancellationToken cancellationToken)
        {
            using var streamReader = new StreamReader(request.SchemaFile);
            var schema = XmlSchema.Read(streamReader, ValidationCallback!)!;

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
            var classDescriptor = new ClassDescriptor(name!, SourceNamespace, InternalNamespace) { Description = complexType.GetDescription() };

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
                        propertyName,
                        type: schemaElement?.GetSchemaType()!,
                        isReferenceType: IsReferenceType(schemaElement!.GetSchemaType()),
                        isArray: schemaElement?.MaxOccursString == "unbounded");
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
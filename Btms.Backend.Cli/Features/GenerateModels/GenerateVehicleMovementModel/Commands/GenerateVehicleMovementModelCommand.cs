using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using CommandLine;
using MediatR;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateVehicleMovementModel.Commands;

[Verb("generate-vehicle-movement-model", isDefault: false,
    HelpText = "Generates Csharp Ipaffs classes from Json Schema.")]
internal class GenerateVehicleMovementModelCommand : IRequest
{
    public const string SourceNamespace = "Btms.Types.Gvms";
    public const string InternalNamespace = "Btms.Model.Gvms";
    public const string SolutionPath = "../../../../";

    public string SchemaFile { get; set; } =
        $"{SolutionPath}/Btms.Backend.Cli/Features/GenerateModels/GenerateVehicleMovementModel/Goods-Vehicle-Movement-Search-1.0-Open-API-Spec.yaml";

    public string SourceOutputPath { get; set; } = $"{SolutionPath}/Btms.Types.Gvms.V1/";

    public string InternalOutputPath { get; set; } = $"{SolutionPath}/Btms.Model/Gvms/";

    public string MappingOutputPath { get; set; } = $"{SolutionPath}/Btms.Types.Gvms.Mapping.V1/";

    public class Handler : IRequestHandler<GenerateVehicleMovementModelCommand>
    {
        public async Task Handle(GenerateVehicleMovementModelCommand request,
            CancellationToken cancellationToken)
        {
            using var streamReader = new StreamReader(request.SchemaFile);
            var reader = new OpenApiStreamReader();
            var document = reader.Read(streamReader.BaseStream, out _);

            var csharpDescriptor = new CSharpDescriptor();

            foreach (var schemas in document.Components.Schemas)
            {
                if (schemas.Key.EndsWith("request", StringComparison.InvariantCultureIgnoreCase) ||
                    schemas.Key.EndsWith("response", StringComparison.InvariantCultureIgnoreCase))


                    BuildClass(csharpDescriptor, schemas.Key, schemas.Value);
            }

            await CSharpFileBuilder.Build(csharpDescriptor, request.SourceOutputPath, request.InternalOutputPath,
                request.MappingOutputPath);
        }

        private void BuildClass(CSharpDescriptor cSharpDescriptor, string name, OpenApiSchema schema)
        {
            var classDescriptor = new ClassDescriptor(name, SourceNamespace, InternalNamespace) { Description = schema.Description };

            cSharpDescriptor.AddClassDescriptor(classDescriptor);

            foreach (var property in schema.Properties)
            {
                if (property.Value.IsArray())
                {
                    var arrayType = property.Value.GetArrayType();

                    if (arrayType == "object")
                    {
                        var propertyDescriptor = new PropertyDescriptor(
                            property.Key,
                            type: ClassDescriptor.BuildClassName(property.Key),
                            isReferenceType: true,
                            isArray: true)
                        {
                            Description = property.Value.Description
                        };

                        classDescriptor.Properties.Add(propertyDescriptor);

                        BuildClass(cSharpDescriptor, property.Key, property.Value.Items);
                    }
                    else if (arrayType == "string")
                    {
                        var propertyDescriptor = new PropertyDescriptor(
                            property.Key,
                            type: "string",
                            isReferenceType: false,
                            isArray: true)
                        {
                            Description = property.Value.Description
                        };

                        classDescriptor.Properties.Add(propertyDescriptor);
                    }
                    else
                        throw new NotImplementedException($"{arrayType} is not implemented");
                }
                else if (property.Value.IsObject())
                {
                    var propertyDescriptor = new PropertyDescriptor(
                        property.Key,
                        type: ClassDescriptor.BuildClassName(property.Key),
                        isReferenceType: true,
                        isArray: false)
                    {
                        Description = property.Value.Description
                    };

                    classDescriptor.Properties.Add(propertyDescriptor);

                    BuildClass(cSharpDescriptor, property.Key, property.Value);
                }
                else if (property.Value.OneOf.Any())
                {
                    var enumDescriptor = new EnumDescriptor(property.Key, null!, SourceNamespace, InternalNamespace);

                    cSharpDescriptor.AddEnumDescriptor(enumDescriptor);

                    foreach (var oneOfSchema in property.Value.OneOf)
                    {
                        var values = oneOfSchema.Enum.Select(x => ((OpenApiString)x).Value).ToList();
                        enumDescriptor.AddValues(values.Select(x => new EnumDescriptor.EnumValueDescriptor(x))
                            .ToList());
                    }

                    var propertyDescriptor = new PropertyDescriptor(
                        property.Key,
                        type: EnumDescriptor.BuildEnumName(property.Key, null!),
                        isReferenceType: true,
                        isArray: false)
                    {
                        Description = property.Value.Description
                    };

                    classDescriptor.Properties.Add(propertyDescriptor);
                }
                else
                {
                    var propertyDescriptor = new PropertyDescriptor(
                        property.Key,
                        type: property.ToCSharpType(),
                        isReferenceType: false,
                        isArray: property.Value.IsArray())
                    {
                        Description = property.Value.Description
                    };

                    classDescriptor.Properties.Add(propertyDescriptor);
                }
            }
        }
    }
}
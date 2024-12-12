using Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;
using CommandLine;
using MediatR;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Commands;

[Verb("generate-ipaffs-model", isDefault: false, HelpText = "Generates Csharp Ipaffs classes from Json Schema.")]
internal class GenerateIpaffsModelCommand : IRequest
{
    // [Option('s', "schema", Required = true, HelpText = "The Json schema file, which to use to generate the csharp classes.")]
    public string SchemaFile { get; set; } = Path.Combine(RootPaths.BackendCliFolder, "Features", "GenerateModels", "GenerateIpaffsModel", "jsonschema.json");

    // [Option('o', "sourceOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string SourceOutputPath { get; set; } = $"{RootPaths.TypesPartialFolder}.Ipaffs.V1{Path.DirectorySeparatorChar}";

    // [Option('i', "internalOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string InteralOutputPath { get; set; } = Path.Combine(RootPaths.ModelFolder, $"Ipaffs{Path.DirectorySeparatorChar}");

    // [Option('i', "internalOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string MappingOutputPath { get; set; } = $"{RootPaths.TypesPartialFolder}.Ipaffs.Mapping.V1{Path.DirectorySeparatorChar}";

    public class Handler : AsyncRequestHandler<GenerateIpaffsModelCommand>
    {
        protected override async Task Handle(GenerateIpaffsModelCommand request, CancellationToken cancellationToken)
        {
            var builder = new IpaffsDescriptorBuilder([new DescriptorBuilderSchemaVisitor()]);

            var model = builder.Build(await File.ReadAllTextAsync(request.SchemaFile, cancellationToken));

            await CSharpFileBuilder.Build(model, request.SourceOutputPath, request.InteralOutputPath, request.MappingOutputPath);
        }
    }
}
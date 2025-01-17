using Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;
using CommandLine;
using MediatR;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Commands;

[Verb("generate-ipaffs-model", isDefault: false, HelpText = "Generates Csharp Ipaffs classes from Json Schema.")]
internal class GenerateIpaffsModelCommand : IRequest
{
    [Option('s', "schema", Required = true,
        HelpText = "The Json schema file, which to use to generate the csharp classes.")]
    public string SchemaFile { get; set; } = null!;

    // [Option('o', "sourceOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string SourceOutputPath { get; set; } = "D:\\repos\\esynergy\\btms-backend\\Btms.Types.Ipaffs.V1\\";

    // [Option('i', "internalOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string InteralOutputPath { get; set; } = "D:\\repos\\esynergy\\btms-backend\\Btms.Model\\Ipaffs\\";

    // [Option('i', "internalOutputPath", Required = true, HelpText = "The path to save the generated csharp classes.")]
    public string MappingOutputPath { get; set; } =
        "D:\\repos\\esynergy\\btms-backend\\Btms.Types.Ipaffs.Mapping.V1\\";

    public class Handler : IRequestHandler<GenerateIpaffsModelCommand>
    {
        public async Task Handle(GenerateIpaffsModelCommand request,
            CancellationToken cancellationToken)
        {
            var builder =
                new IpaffsDescriptorBuilder([new DescriptorBuilderSchemaVisitor()]);

            var model = builder.Build(await File.ReadAllTextAsync(request.SchemaFile, cancellationToken));

            await CSharpFileBuilder.Build(model, request.SourceOutputPath, request.InteralOutputPath,
                request.MappingOutputPath);
        }
    }
}
using Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Builders;
using CommandLine;
using MediatR;

namespace Btms.Backend.Cli.Features.GenerateModels.GenerateIpaffsModel.Commands;

[Verb("generate-ipaffs-model", isDefault: false, HelpText = "Generates Csharp Ipaffs classes from Json Schema.")]
internal class GenerateIpaffsModelCommand : IRequest
{
    public const string SourceNamespace = "Btms.Types.Ipaffs";
    public const string InternalNamespace = "Btms.Model.Ipaffs";
    public const string ClassNamePrefix = "";
    public const string SolutionPath = "../../../../";

    [Option('s', "schema",
        HelpText = "The Json schema file, which to use to generate the csharp classes.")]
    public string SchemaFile { get; set; } =
        $"{SolutionPath}/Btms.Backend.Cli/Features/GenerateModels/GenerateIpaffsModel/notification-schema-17.5.json";

    public string SourceOutputPath { get; set; } = $"{SolutionPath}/Btms.Types.Ipaffs.V1/";

    public string InternalOutputPath { get; set; } = $"{SolutionPath}/Btms.Model/Ipaffs/";

    public string MappingOutputPath { get; set; } = $"{SolutionPath}/Btms.Types.Ipaffs.Mapping.V1/";

    public class Handler : IRequestHandler<GenerateIpaffsModelCommand>
    {
        public async Task Handle(GenerateIpaffsModelCommand request,
            CancellationToken cancellationToken)
        {
            var builder =
                new IpaffsDescriptorBuilder([new DescriptorBuilderSchemaVisitor()]);

            var model = builder.Build(await File.ReadAllTextAsync(request.SchemaFile, cancellationToken));

            await CSharpFileBuilder.Build(model, request.SourceOutputPath, request.InternalOutputPath,
                request.MappingOutputPath);
        }
    }
}
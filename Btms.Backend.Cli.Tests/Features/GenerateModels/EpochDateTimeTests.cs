using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;

namespace Btms.Backend.Cli.Tests.Features.GenerateModels;

public class EpochDateTimeTests(CodeBuilderFixture fixture) : IClassFixture<CodeBuilderFixture>
{   
    private async Task<CSharpDescriptor> BuildEpochDateProperty()
    {
        var csharpDescriptor = await fixture.BuildSingleProperty(
            "Header",
            new PropertyDescriptor(
                "arrivalDateTime",
                type: "DateTime",
                isReferenceType: false,
                isArray: false)
            {
                DateTimeType = DateTimeType.Epoch
            }
        );
        return csharpDescriptor;
    }

    [Fact]
    public async Task ShouldProduce3Files()
    {
        var csharpDescriptor = await BuildEpochDateProperty();

        csharpDescriptor.OutputFiles.Count.Should().Be(3);
    }

    [Fact]
    public async Task ShouldAddLocalDateTimeJsonConverterAttributeToSourceType()
    {
        var csharpDescriptor = await BuildEpochDateProperty();
        
        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Path == "/tmp/btms-cli-tests/source/Header.g.cs");
        
        sourceFile.Content.Should().Contain("[Btms.Common.Json.EpochDateTimeJsonConverter]");
    }

    [Fact]
    public async Task ShouldNotAddLocalDateTimeJsonConverterAttributeToInternalModel()
    {
        var csharpDescriptor = await BuildEpochDateProperty();
        
        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Path == "/tmp/btms-cli-tests/internal/Header.g.cs");
        
        sourceFile.Content.Should().NotContain("Btms.Common.Json.EpochDateTimeJsonConverter");
    }
}
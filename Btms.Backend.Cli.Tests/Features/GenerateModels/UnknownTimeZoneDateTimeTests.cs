using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;

namespace Btms.Backend.Cli.Tests.Features.GenerateModels;

public class UnknownTimeZoneDateTimeTests(CodeBuilderFixture fixture) : IClassFixture<CodeBuilderFixture>
{
    private async Task<CSharpDescriptor> BuildLocalDateProperty()
    {
        var csharpDescriptor = await fixture.BuildSingleProperty(
            "ActualCrossing",
            new PropertyDescriptor(
                "arrivesAt",
                type: "DateTime",
                isReferenceType: false,
                isArray: false)
            {
                DateTimeType = DateTimeType.Local
            }
        );
        return csharpDescriptor;
    }

    [Fact]
    public async Task ShouldProduce3Files()
    {
        var csharpDescriptor = await BuildLocalDateProperty();

        csharpDescriptor.OutputFiles.Count.Should().Be(3);
    }

    [Fact]
    public async Task ShouldAddLocalDateTimeJsonConverterAttributeToSourceType()
    {
        var csharpDescriptor = await BuildLocalDateProperty();

        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Path == "/tmp/btms-cli-tests/source/ActualCrossing.g.cs");

        sourceFile.Content.Should().Contain("[Btms.Common.Json.UnknownTimeZoneDateTimeJsonConverter(nameof(ArrivesAt))");
    }

    [Fact]
    public async Task ShouldAddLocalDateTimeJsonConverterAttributeToInternalModel()
    {
        var csharpDescriptor = await BuildLocalDateProperty();

        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Path == "/tmp/btms-cli-tests/internal/ActualCrossing.g.cs");

        sourceFile.Content.Should().Contain("[Btms.Common.Json.UnknownTimeZoneDateTimeJsonConverter(nameof(ArrivesAt)), MongoDB.Bson.Serialization.Attributes.BsonDateTimeOptions(Kind = DateTimeKind.Unspecified)]");
    }
}
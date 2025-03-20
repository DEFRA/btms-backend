using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;

namespace Btms.Backend.Cli.Tests.Features.GenerateModels;

public class NullabilityTests(CodeBuilderFixture fixture) : IClassFixture<CodeBuilderFixture>
{
    [Fact]
    public async Task NullablePropertyInMapper()
    {
        var csharpDescriptor = await fixture.BuildSingleProperty(
            "PlannedCrossing",
            new PropertyDescriptor(
                "routeId",
                type: "string",
                isReferenceType: false,
                isArray: false)
        );

        csharpDescriptor.OutputFiles.Count.Should().Be(3);

        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Name == "PlannedCrossingMapper");

        sourceFile.Content.Should().Contain("public static Test.Model.PlannedCrossing Map(Test.Source.PlannedCrossing? from)");
        sourceFile.Content.Should().Contain("to.RouteId = from?.RouteId");
    }
}
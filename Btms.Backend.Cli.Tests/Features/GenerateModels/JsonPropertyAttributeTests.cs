using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;

namespace Btms.Backend.Cli.Tests.Features.GenerateModels;

public class JsonPropertyAttributeTests(CodeBuilderFixture fixture) : IClassFixture<CodeBuilderFixture>
{

    [Fact]
    public async Task RenamedProperty()
    {
        var csharpDescriptor = await fixture.BuildSingleProperty("PlannedCrossing",
            new PropertyDescriptor(
                "localDateTimeOfDeparture",
                type: "string",
                isReferenceType: false,
                isArray: false)
        );
        //
        //
        // var csharpDescriptor = new CSharpDescriptor();
        // var classDescriptor = new ClassDescriptor("PlannedCrossing", "Test.Source", "Test.Model", "");
        // var propertyDescriptor = new PropertyDescriptor(
        //     "localDateTimeOfDeparture",
        //     type: "string",
        //     isReferenceType: false,
        //     isArray: false);

        // classDescriptor.Properties.Add(propertyDescriptor);
        //
        // csharpDescriptor.AddClassDescriptor(classDescriptor);
        //
        // await CSharpFileBuilder.Generate(csharpDescriptor, "/tmp/btms-cli-tests/source", "/tmp/btms-cli-tests/internal","/tmp/btms-cli-tests-mapping/");

        csharpDescriptor.OutputFiles.Count.Should().Be(3);

        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Path == "/tmp/btms-cli-tests/source/PlannedCrossing.g.cs");
        
        sourceFile.Content.Should().Contain("public DateTime? DepartsAt { get; set; }");
        sourceFile.Content.Should().Contain("[JsonPropertyName(\"localDateTimeOfDeparture\")]");
    }
}
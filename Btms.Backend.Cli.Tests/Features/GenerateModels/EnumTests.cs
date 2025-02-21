using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.ClassMaps;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;

namespace Btms.Backend.Cli.Tests.Features.GenerateModels;

public class EnumTests(CodeBuilderFixture fixture) : IClassFixture<CodeBuilderFixture>
{
    [Fact]
    public async Task ShouldSupportAddingEnumItems()
    {
        var csharpDescriptor = await fixture.BuildEnum(
            new EnumDescriptor(
                "PurposePurposeGroup",
                null,
                "Source",
                "Internal")
            {
                Values = [new EnumDescriptor.EnumValueDescriptor("For Import")]
            }
        );
        
        csharpDescriptor.OutputFiles.Count.Should().Be(3);
        
        var sourceFile =
            csharpDescriptor.OutputFiles.Single(f =>
                f.Path == "/tmp/btms-cli-tests/source/PurposePurposeGroupEnum.g.cs");
        
        sourceFile.Content.Should().Contain("ForImportNonInternalMarket");
        sourceFile.Content.Should().Contain("[EnumMember(Value = \"For Import Non-Internal Market\")]");
        sourceFile.Content.Should().Contain("[EnumMember(Value = \"For Import\")]");
    }
}
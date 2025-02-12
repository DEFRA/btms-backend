using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;

namespace Btms.Backend.Cli.Tests.Features.GenerateModels;

public class JsonPropertyAttributeTests
{
    
    [Fact]
    public async Task Test1()
    {
        Setup.Initialise();
        var csharpDescriptor = new CSharpDescriptor();
        var classDescriptor = new ClassDescriptor("JsonPropertyAttributeTest", "Test.Source", "Test.Model", "");
        var propertyDescriptor = new PropertyDescriptor(
            sourceName: "localDateTimeOfDeparture",
            type: "string",
            description: "The planned date and time of departure, in local...",
            isReferenceType: false,
            isArray: false,
            classNamePrefix: "");

        classDescriptor.Properties.Add(propertyDescriptor);
        
        csharpDescriptor.AddClassDescriptor(classDescriptor);
        
        await CSharpFileBuilder.Generate(csharpDescriptor, "/tmp/btms-cli-tests/source", "/tmp/btms-cli-tests/internal","/tmp/btms-cli-tests-mapping/");

        csharpDescriptor.OutputFiles.Count.Should().Be(1);
    }
}
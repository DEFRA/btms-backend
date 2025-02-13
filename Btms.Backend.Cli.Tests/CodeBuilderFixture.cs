using Btms.Backend.Cli.Features.GenerateModels;
using Btms.Backend.Cli.Features.GenerateModels.DescriptorModel;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Btms.Backend.Cli.Tests;

public class CodeBuilderFixture
{
    public CodeBuilderFixture()
    {
        Console.WriteLine("CodeBuilderFixture ctr");
    }
    static CodeBuilderFixture()
    {
        Console.WriteLine("CodeBuilderFixture static ctr");
        Scope = Setup.Initialise();
    }
    
    public static IServiceScope Scope { get; private set; }

    public async Task<CSharpDescriptor> BuildSingleProperty(string className, PropertyDescriptor propertyDescriptor)
    {
        var csharpDescriptor = new CSharpDescriptor();
        var classDescriptor = new ClassDescriptor(className, "Test.Source", "Test.Model", "");
        
        classDescriptor.Properties.Add(propertyDescriptor);
        
        csharpDescriptor.AddClassDescriptor(classDescriptor);
        
        await CSharpFileBuilder.Generate(csharpDescriptor, "/tmp/btms-cli-tests/source", "/tmp/btms-cli-tests/internal","/tmp/mapping/");

        return csharpDescriptor;
    }
}

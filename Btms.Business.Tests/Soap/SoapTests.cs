using System.Text.Json;
using System.Text.Json.Serialization;
using Btms.Types.Ipaffs.Mapping;
using TestDataGenerator.Scenarios;
using Xunit;
using Xunit.Abstractions;

namespace Btms.Business.Tests.Soap;

public class SoapTests(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("cheda")]
    [InlineData("chedd")]
    [InlineData("chedp")]
    [InlineData("chedpp")]
    public void GenerateSoapAndConvertToModel(string type)
    {
        var generator = new SoapGenerator(type);
        var config = ScenarioFactory.CreateScenarioConfig(generator, 1, 1);
        var data = generator.Generate(1, 1, DateTime.Now, config);
        var notification = (Types.Ipaffs.ImportNotification?) data.FirstOrDefault();

        Assert.NotNull(notification);
        
        var model = notification.MapWithTransform();
        
        Assert.NotNull(model);

        var serialized = JsonSerializer.Serialize(model, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        });
        
        testOutputHelper.WriteLine(serialized);
    }
}
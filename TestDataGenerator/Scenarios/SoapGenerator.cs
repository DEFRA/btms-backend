namespace TestDataGenerator.Scenarios;

public class SoapGenerator(string type = "cheda") : ScenarioGenerator
{
    public override GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config)
    {
        var notification = GetNotificationBuilder($"soap-{type}");
        
        return new GeneratorResult([notification.ValidateAndBuild()]);
    }
}
using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;
using TestDataGenerator.Scenarios;

namespace TestDataGenerator;

public abstract class ScenarioGenerator
{
    private readonly string _fullFolder =
        $"{Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)}/Scenarios/Samples";

    public abstract GeneratorResult Generate(int scenario, int item, DateTime entryDate, ScenarioConfig config);

    internal ImportNotificationBuilder<ImportNotification> GetNotificationBuilder(string file)
    {
        var fullPath = $"{_fullFolder}/{file}.json";
        var builder = ImportNotificationBuilder.FromFile(fullPath);

        return builder;
    }

    internal ClearanceRequestBuilder GetClearanceRequestBuilder(string file)
    {
        var fullPath = $"{_fullFolder}/{file}.json";
        var builder = new ClearanceRequestBuilder(fullPath);

        return builder;
    }

    public class GeneratorResult
    {
        public ImportNotification[] ImportNotifications { get; set; } = default!;
        public AlvsClearanceRequest[] ClearanceRequests { get; set; } = default!;
    }
}
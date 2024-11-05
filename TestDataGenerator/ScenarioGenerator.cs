using Cdms.Types.Alvs;
using Cdms.Types.Ipaffs;

namespace TestDataGenerator;

public abstract class ScenarioGenerator()
{
    private string fullFolder = $"{Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory)}/Scenarios/Samples";

    internal class GeneratorResult
    {
        public ImportNotification[] ImportNotifications { get; set; } = default!;
        public AlvsClearanceRequest[] ClearanceRequests { get; set; } = default!;
    }
    
    // Not sure if this should be abstract or not...
    internal abstract GeneratorResult Generate(int item, DateTime entryDate);

    internal ImportNotificationBuilder<ImportNotification> GetNotificationBuilder(string file)
    {
        var fullPath = $"{fullFolder}/{file}.json";
        var builder = ImportNotificationBuilder.FromFile(fullPath);
        
        return builder;
    }
    
    internal ClearanceRequestBuilder GetClearanceRequestBuilder(string file)
    {
        var fullPath = $"{fullFolder}/{file}.json";
        var builder = new ClearanceRequestBuilder(fullPath);
        
        return builder;
    }
}
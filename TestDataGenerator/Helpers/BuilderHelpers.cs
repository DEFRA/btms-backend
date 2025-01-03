using Btms.Types.Ipaffs;

namespace TestDataGenerator.Helpers;

public static class BuilderHelpers
{
    private static readonly string fullFolder =
        $"{Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)}/Scenarios/Samples";
    
    internal static ClearanceRequestBuilder GetClearanceRequestBuilder(string file, string fileExtension = ".json")
    {
        // return BuilderHelpers.GetClearanceRequestBuilder(file, fileExtension);
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = new ClearanceRequestBuilder(fullPath);
        
        return builder;
    }
    
    internal static DecisionBuilder GetDecisionBuilder(string file, string fileExtension = ".json")
    {
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = new DecisionBuilder(fullPath);

        return builder;
    }
    
    internal static ImportNotificationBuilder<ImportNotification> GetNotificationBuilder(string file, string fileExtension = ".json")
    {
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = ImportNotificationBuilder.FromFile(fullPath);

        return builder;
    }
}
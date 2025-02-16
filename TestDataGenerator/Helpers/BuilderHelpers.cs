using Btms.Types.Ipaffs;

namespace TestDataGenerator.Helpers;

public static class BuilderHelpers
{
    private static readonly string fullFolder =
        $"{Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)}/Scenarios/Samples";
    private const string JSON_FILE_EXTENSION = ".json";

    internal static ClearanceRequestBuilder GetClearanceRequestBuilder(string file, string fileExtension = JSON_FILE_EXTENSION)
    {
        // return BuilderHelpers.GetClearanceRequestBuilder(file, fileExtension);
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = new ClearanceRequestBuilder(fullPath);

        return builder;
    }

    internal static FinalisationBuilder GetFinalisationBuilder(string file, string fileExtension = JSON_FILE_EXTENSION)
    {
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = new FinalisationBuilder(fullPath);

        return builder;
    }

    internal static DecisionBuilder GetDecisionBuilder(string file, string fileExtension = JSON_FILE_EXTENSION)
    {
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = new DecisionBuilder(fullPath);

        return builder;
    }

    internal static ImportNotificationBuilder<ImportNotification> GetNotificationBuilder(string file, string fileExtension = JSON_FILE_EXTENSION)
    {
        var fullPath = $"{fullFolder}/{file}{fileExtension}";
        var builder = ImportNotificationBuilder.FromFile(fullPath);

        return builder;
    }
}
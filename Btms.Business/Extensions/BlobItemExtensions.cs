using System.Text.Json.Nodes;
using Btms.BlobService;
using Btms.Types.Alvs;
using Json.Path;

namespace Btms.Business.Extensions;

public static class BlobItemExtensions
{
    private static string[] GetCleanPathParts(this IBlobItem item)
    {
        //Get the parts of the path as an array, dropping the DmpBlobRootFolder part
        return item.Name
            .Split('/')
            .Skip(1)
            .ToArray();
    }


    // CDMS-408 'Temporary' fix for incorrect paths for ALVS in data lake
    // Move files into the correct folder by checking if JSON elements exists
    public static (Type, string[]) EnsureCorrectTypeAndFilePath(this IBlobItem item, Type type, string content)
    {
        var fileParts = item.GetCleanPathParts();
        var path = JsonPath.Parse("$.header.finalState", new PathParsingOptions { AllowMathOperations = true });
        var expected = JsonNode.Parse(content);
        if (path.Evaluate(expected).Matches.Count > 0)
        {
            fileParts[0] = "FINALISATION";
            return (typeof(Finalisation), fileParts);
        }

        path = JsonPath.Parse("$.header.decisionNumber", new PathParsingOptions { AllowMathOperations = true });
        expected = JsonNode.Parse(content);
        if (path.Evaluate(expected).Matches.Count > 0)
        {
            fileParts[0] = "DECISIONS";
            return (typeof(Decision), fileParts);
        }

        return (type, fileParts);
    }

}
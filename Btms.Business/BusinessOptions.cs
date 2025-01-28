using System.ComponentModel.DataAnnotations;
using Btms.Business.Commands;

namespace Btms.Business;

public class BusinessOptions
{
    public const string SectionName = nameof(BusinessOptions);

    private readonly int defaultDegreeOfParallelism = Math.Max(Environment.ProcessorCount / 4, 1);
    
    [Required] public string DmpBlobRootFolder { get; set; } = "RAW";

    public Dictionary<string, Dictionary<Feature, int>> ConcurrencyConfiguration { get; set; }

    public enum Feature
    {
        BlobPaths,
        BlobItems
    }
    public BusinessOptions()
    {
        ConcurrencyConfiguration = new Dictionary<string, Dictionary<Feature, int>>
        {
            {
                nameof(SyncNotificationsCommand), new Dictionary<Feature, int>()
                {
                    { Feature.BlobPaths, defaultDegreeOfParallelism }, { Feature.BlobItems, defaultDegreeOfParallelism }
                }
            },
            {
                nameof(SyncClearanceRequestsCommand), new Dictionary<Feature, int>()
                {
                    { Feature.BlobPaths, defaultDegreeOfParallelism }, { Feature.BlobItems, defaultDegreeOfParallelism }
                }
            },
            {
                nameof(SyncDecisionsCommand), new Dictionary<Feature, int>()
                {
                    { Feature.BlobPaths, defaultDegreeOfParallelism }, { Feature.BlobItems, defaultDegreeOfParallelism }
                }
            },
            {
                nameof(SyncFinalisationsCommand), new Dictionary<Feature, int>()
                {
                    { Feature.BlobPaths, defaultDegreeOfParallelism }, { Feature.BlobItems, defaultDegreeOfParallelism }
                }
            }
        };
    }

    public int GetConcurrency<T>(Feature feature)
    {
        if (ConcurrencyConfiguration.TryGetValue(typeof(T).Name, out var degreeOfParallelismDictionary))
        {
            return degreeOfParallelismDictionary.GetValueOrDefault(feature, defaultDegreeOfParallelism);
        }
        return defaultDegreeOfParallelism;
    }
}
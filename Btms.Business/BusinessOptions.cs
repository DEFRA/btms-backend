using System.ComponentModel.DataAnnotations;
using Btms.Azure;

namespace Btms.Business;

public class BusinessOptions
{
    public const string SectionName = nameof(BusinessOptions);

    [Required] public string DmpBlobRootFolder { get; set; } = "RAW";
    
    public Dictionary<string, int>? MaxDegreeOfParallelism { get; set; }
}
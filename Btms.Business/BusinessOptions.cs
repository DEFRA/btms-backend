using System.ComponentModel.DataAnnotations;

namespace Btms.Business;

public class BusinessOptions
{
    public const string SectionName = nameof(BusinessOptions);

    [Required] public string DmpBlobRootFolder { get; set; } = "RAW";
    
}
using System.ComponentModel.DataAnnotations;
using Cdms.Azure;

namespace Cdms.Business;

public class BusinessOptions
{
    public const string SectionName = nameof(BusinessOptions);

    [Required] public string DmpBlobRootFolder { get; set; } = "Raw";
    
}